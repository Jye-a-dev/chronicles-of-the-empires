# BÁO CÁO CHẨN ĐOÁN LỖI DE-SYNC DI CHUYỂN UNIT

**Dự án:** Chronicles of the Empires  
**Môi trường:** Godot 4 .NET (C# / .NET 8)  
**Phân hệ:** Gameplay / Hexagonal Tactical Grid (32x32)

---

## 1. PHÂN TÍCH NGUYÊN NHÂN CỐT LÕI (ROOT CAUSE AUDIT)

Sau khi rà soát toàn bộ cây Scene (`unit.tscn`, `world_map.tscn`), luồng Model-View (`UnitData.cs`, `UnitController.cs`), và bộ điều phối bàn cờ (`WorldMap.cs`, `MapInputHandler.cs`, `PathfindingManager.cs`), các nguyên nhân kỹ thuật cốt lõi gây ra hiện tượng de-sync (Hitbox/A*/Picking nhảy tới ô đích ngay lập tức trong khi Visual Sprite bị kẹt ở ô cũ) gồm:

### Điểm nghẽn 1: Premature Snap ngược về ô cũ trong `TweenCallback`
- **Vị trí:** `UnitController.cs` (dòng 442–454) kết hợp với `WorldMap.cs` (dòng 436–465).
- **Cơ chế phát sinh:**
  1. Trong `WorldMap.ExecuteSafeMoveTransactionalAsync`:
     - Tọa độ logic `unit.Data.GridPosition` giữ nguyên giá trị `originGrid`.
     - Gọi `unit.MoveAlongPath(path, () => tcs.TrySetResult(true))` rồi `await tcs.Task`.
  2. Trong `UnitController.MoveAlongPath`:
     - Tween nội suy `this.Position` trượt qua các ô cho tới đích.
     - Khi Tween hoàn tất, `TweenCallback` kích hoạt trước khi `onComplete` kịp trả về cho `WorldMap`:
       ```csharp
       _movementTween.TweenCallback(Callable.From(() =>
       {
           Data.IsMoving = false;
           Position = GridMapManager.GridToWorldCenter(Data.GridPosition); // <-- ĐIỂM GÃY LOGIC
           ...
           onComplete?.Invoke();
       }));
       ```
     - Tại thời điểm này, `Data.GridPosition` **vẫn là `originGrid`** (ô xuất phát) do `WorldMap` chưa được đánh thức để cập nhật `targetGrid`.
     - Hệ quả: Ngay frame cuối cùng khi vừa lướt tới đích, `Position` của `UnitController` lập tức bị **ép giật ngược về tâm ô cũ (`originGrid`)**.
  3. Khi `onComplete` kích hoạt `tcs.TrySetResult(true)`:
     - `WorldMap` thức dậy, chuyển `originCell.OccupyingUnit = null`, `targetCell.OccupyingUnit = unit`, `unit.Data.GridPosition = targetGrid`, và cập nhật `_unitRegistry`.
     - Tuy nhiên, `WorldMap` chỉ cập nhật `unit.Position` khi `!success`:
       ```csharp
       if (!success)
       {
           unit.Position = GridMapManager.GridToWorldCenter(targetGrid);
       }
       ```
     - Vì Tween chạy hoàn tất bình thường (`success == true`), nhánh trên bị bỏ qua, khiến `unit.Position` (và toàn bộ cây con `Sprite2D`, `VisualBorder`, `Polygon2D`) bị bỏ rơi tại `originGrid`.

### Điểm nghẽn 2: Cơ chế Mouse Picking dựa trên tọa độ Grid thay vì Physics Raycast
- Mouse Picking trong `MapInputHandler.cs` (dòng 158, 215) dựa trên phép chiếu:
  ```csharp
  Vector2I gridPos = GridMapManager.WorldToGrid(mouseWorld);
  var clickedUnit = _unitRegistry.GetUnitAt(gridPos) ?? hexCell.OccupyingUnit as UnitController;
  ```
- Khi `targetCell.OccupyingUnit` và `_unitRegistry` đã cập nhật sang `targetGrid`, việc click chuột vào ô đích sẽ nhặt trúng Unit dù Sprite không ở đó; ngược lại, click vào Sprite hiển thị ở ô cũ sẽ trả về `null` vì ô cũ đã bị hủy Occupancy.

### Điểm nghẽn 3: Xung đột Tween trên thuộc tính `"position"` và hủy ngang không an toàn
- Trong `UnitController.cs`, `PlayLowMoraleJitter` can thiệp trực tiếp vào `this, "position"` mà không kiểm tra cờ `IsMoving`. Nếu sự kiện morale thay đổi kích hoạt khi đang di chuyển, Tween jitter sẽ đè lên `_movementTween`.
- Khi `_movementTween.Kill()` được gọi, `TweenCallback` của Godot 4 sẽ không chạy, dẫn đến `Data.IsMoving` bị kẹt `true`, `onComplete` không chạy, gây treo `tcs` 5 giây trước khi timeout kích hoạt.

---

## 2. GIẢI PHÁP TRIỂN KHAI

1. **`UnitController.cs`**:
   - Lưu biến cục bộ `destinationPos = path[^1]`.
   - Trong `TweenCallback`:
     - Cập nhật trực tiếp `Data.GridPosition = destinationPos;`
     - Ép `Position = GridMapManager.GridToWorldCenter(destinationPos);`
     - Cập nhật `Data.IsMoving = false;`
     - Phát tín hiệu `EmitSignal(SignalName.UnitMoved, this, originPos, destinationPos);`
   - Bổ sung `CancelMovement()`: Khi hủy Tween, ép `Position = GridMapManager.GridToWorldCenter(Data.GridPosition);` và hạ cờ `IsMoving = false;`.
   - Chuyển animation rung rẩy `PlayLowMoraleJitter` sang tác động lên node con `_unitSprite` thay vì can thiệp vào `this.Position`.

2. **`WorldMap.cs`**:
   - Chuẩn hóa `ExecuteSafeMoveTransactionalAsync`:
     - Đảm bảo tính toán và truyền đúng tọa độ World Position.
     - Sau khi `await tcs.Task`:
       - Khi `success == true`: Cập nhật logic occupancy, cập nhật `Data.GridPosition = targetGrid`, và ép xác nhận `unit.Position = GridMapManager.GridToWorldCenter(targetGrid)` để đảm bảo tính toàn vẹn 100%.
       - Khi `!success`: Hoàn trả cản A*, gọi `unit.CancelMovement()` để đưa Visual về vị trí an toàn.

