using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public static partial class LocalizationManager
{
    private static readonly Dictionary<string, string> ViTranslations = new()
    {
        // Core Game Title
        ["MENU_TITLE"] = "VẠN QUỐC PHONG VÂN KÝ",
        ["MENU_SUBTITLE"] = "◆ THỜI KỲ HUYỀN SỬ ◆",
        ["MENU_NEW_CAMPAIGN"] = "Chiến Dịch Mới ▸",
        ["MENU_LOAD_GAME"] = "Tải Bản Lưu",
        ["MENU_SETTINGS"] = "Cài Đặt",
        ["MENU_EXIT"] = "Thoát Trò Chơi",
        ["MENU_ARCHIVES"] = "📖 Thư Khố",
        ["MENU_CREDITS"] = "👥 Đội Ngũ",
        ["MENU_DISCORD"] = "💬 Cộng Đồng",
        ["MENU_ARCHIVES_TOOLTIP"] = "Thư Khố & Bách Khoa Toàn Thư Vạn Quốc",
        ["MENU_CREDITS_TOOLTIP"] = "Đội Ngũ Phát Triển & Vinh Danh",
        ["MENU_DISCORD_TOOLTIP"] = "Kênh Discord Cộng Đồng Chính Thức",

        // Campaign Mode Selector
        ["CAMPAIGN_MODAL_TITLE"] = "QUÂN CƠ DOANH & ĐIỀU ĐỘNG VẠN BANG",
        ["CAMPAIGN_HEADER"] = "QUÂN CƠ DOANH & ĐIỀU ĐỘNG VẠN BANG",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "⚔ Thiên Sử Thi Vạn Quốc",
        ["CAMPAIGN_TAB_SKIRMISH"] = "⚡ Quần Hùng Tranh Phong",
        ["CAMPAIGN_TAB_TRAINING"] = "📜 Binh Pháp & Diễn Võ Trường",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ Thiên Sử Thi Vạn Quốc",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ Quần Hùng Tranh Phong",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Diễn Võ Điểm Binh",
        ["CAMPAIGN_DEPLOY"] = "⚔ HẠ LỆNH XUẤT CHINH",
        ["CAMPAIGN_START_SKIRMISH"] = "⚡ KHỞI CHIẾN TRANH BÁ",
        ["CAMPAIGN_START_TRAINING"] = "📜 NHẬP TRẬN DIỄN VÕ",
        ["SPEC_MAP_PREFIX"] = "Chiến địa: ",
        ["SPEC_RIVALS_PREFIX"] = "Kình địch: ",
        ["SPEC_VICTORY_PREFIX"] = "Chiến lược: ",
        ["SPEC_REWARD_PREFIX"] = "Chiến lợi: ",

        // Mode Details for CampaignModePanel
        ["MODE_GRAND_NAME"] = "Thiên Sử Thi Vạn Quốc",
        ["MODE_GRAND_ERA"] = "Kỷ Nguyên Tứ Đại Cương Vực (Đại Thao Lược Vĩ Mô & Chỉ Huy Chiến Trận Tạm Dừng)",
        ["MODE_GRAND_DESC"] = "Khởi lập vương triều thần thoại, kinh qua bốn khối văn minh vĩ đại của cổ giới. Khai hoang lưu vực sông mẹ, vượt bão cát cơ giới, phá vỡ cự thạch khiên giáo và chinh phục lãnh nguyên băng giá để đăng cơ Bá Chủ Thiên Hạ.",
        ["MODE_GRAND_MAP"] = "Toàn Cảnh Đại Lục Tranh Cuộn Cổ Họa",
        ["MODE_GRAND_RIVALS"] = "Mười Tám Vương Triều Thuộc Tứ Đại Văn Khối",
        ["MODE_GRAND_VICTORY"] = "Bá Nghiệp Thiên Hạ hoặc Thần Khí Trấn Quốc",

        ["MODE_SKIRMISH_NAME"] = "Quần Hùng Tranh Phong",
        ["MODE_SKIRMISH_ERA"] = "Sa Bàn Tùy Biến (Tự Do Thao Lược)",
        ["MODE_SKIRMISH_DESC"] = "Khai màn huyết chiến tức thì trên các sa bàn địa đồ phân vùng hoặc lưới thoi cổ phong. Tự do ấn định thế lực kình địch, hình thế non sông và điều kiện định đoạt thắng bại.",
        ["MODE_SKIRMISH_MAP"] = "Hình Thế Sa Bàn (Tiểu Cương Vực, Trung Sa Bàn, Đại Lục Bao La)",
        ["MODE_SKIRMISH_RIVALS"] = "2 đến 8 Đại Đế Bang Tranh Đoạt",
        ["MODE_SKIRMISH_VICTORY"] = "Bá Nghiệp Thôn Tính, Kỳ Đài Thần Khí, Trảm Tướng Đoạt Kỳ",

        ["MODE_TUTORIAL_NAME"] = "Binh Pháp Điểm Binh & Diễn Võ Trường",
        ["MODE_TUTORIAL_ERA"] = "Thao Trường Diễn Tập & Khảo Nghiệm Quân Cơ",
        ["MODE_TUTORIAL_DESC"] = "Thấu suốt thuật điều phối các trung đoàn tác chiến, chủ động vận dụng quân lệnh Tạm Dừng Tác Chiến (Phím Cách), khai thác địa hình cao hiểm và nắm vững tương sinh tương khắc giữa các binh chủng.",
        ["MODE_TUTORIAL_MAP"] = "Võ Trường Điểm Binh & Đấu Sa Bàn Đẳng Cự",
        ["MODE_TUTORIAL_RIVALS"] = "1 Đại Đội Diễn Tập Đối Kháng",
        ["MODE_TUTORIAL_VICTORY"] = "Lĩnh Hội Toàn Vẹn Binh Pháp",

        // Cultural Blocks
        ["BLOCK_ALLUVIUM_NAME"] = "Bách Thần & Phù Sa",
        ["BLOCK_ALLUVIUM_REGION"] = "Lưu Vực Đại Hà & Sa Mạc Cổ Giới",
        ["BLOCK_ALLUVIUM_DESC"] = "Đồng bằng châu thổ phì nhiêu, phòng tuyến lòng sông cổ kính, đền tháp linh thiêng và nghi lễ huyền bí.",

        ["BLOCK_MANDATE_NAME"] = "Thiết Kỷ & Thiên Mệnh",
        ["BLOCK_MANDATE_REGION"] = "Cửu Châu Lục Thổ & Thảo Nguyên Bao La",
        ["BLOCK_MANDATE_DESC"] = "Kỷ luật giáp sắt nghiêm ngặt, hỏa xa công thành, kỵ binh du mục và thiên mệnh hoàng gia.",

        ["BLOCK_STEEL_NAME"] = "Thánh Huy & Trọng Thép",
        ["BLOCK_STEEL_REGION"] = "Biển Ngọc Hải Ngạn & Tây Cương Cổ Lục",
        ["BLOCK_STEEL_DESC"] = "Trận đồ giáo khiên bất khả xâm phạm, cú húc kỵ sĩ hoàng gia, bão tên trường cung và thiết giáp kiên cố.",

        ["BLOCK_FROST_NAME"] = "Hoang Thần & Băng Lãnh",
        ["BLOCK_FROST_REGION"] = "Lãnh Nguyên Băng Tuyết & Rừng Già Viễn Cổ",
        ["BLOCK_FROST_DESC"] = "Tín ngưỡng đa thần nguyên thủy, chiến binh băng giá, kỵ binh cánh đại bàng và dũng khí rừng thiêng.",

        // 18 Civilizations
        // 1. Lạc Uyên
        ["CIV_VN_NAME"] = "Đế Bang Lạc Uyên (Thủy Thổ Hoàng Triều)",
        ["CIV_VN_EPITHET"] = "Chúa Tể Lưu Vực Xích Hà",
        ["CIV_VN_COLORS"] = "Xanh Rêu Đồng Cổ (#43B3AE) & Vàng Lúa (#E5A93C)",
        ["CIV_VN_ATTIRE"] = "Nón lông chim thần Lạc vuốt nhọn, hộ tâm phiến tròn đúc hoa văn sấm sét, khố gấm dệt tay",
        ["CIV_VN_UNIT"] = "Nỏ Thần Uyên Châu & Tượng Binh Mây Triều Hải",
        ["CIV_VN_TRAIT"] = "Tàng hình trong rừng ngập mặn và đầm lầy, nỏ bắn xuyên giáp, bẫy cọc ngầm lòng sông, buff vĩnh viễn sau lũ lụt",

        // 2. Baray-Nagar
        ["CIV_KHMER_NAME"] = "Thánh Bang Baray-Nagar (Đế Chế Thần Hồ Sovannas)",
        ["CIV_KHMER_EPITHET"] = "Kiến Trúc Gia Thần Hồ Cổ Cảnh",
        ["CIV_KHMER_COLORS"] = "Vàng Sa Khoáng (#D4AF37) & Nâu Đất Nung (#8B4513)",
        ["CIV_KHMER_ATTIRE"] = "Vải quấn Sampot dệt chỉ vàng, giáp ngực nung khắc tượng tiên nữ Apsara thần tích, khắc bùa hộ mệnh toàn thân",
        ["CIV_KHMER_UNIT"] = "Kim Tượng Ngà Vàng & Vệ Binh Đền Thánh Baray",
        ["CIV_KHMER_TRAIT"] = "Hệ thống thủy lợi Baray cấp lương cực hạn vùng khô cằn, đền tháp tăng cường sinh lực và phòng thủ",

        // 3. Solarika
        ["CIV_INDIA_NAME"] = "Thần Triều Luân Xa Solarika (Thái Dương Mauraka)",
        ["CIV_INDIA_EPITHET"] = "Người Nắm Giữ Luân Xa Thái Dương",
        ["CIV_INDIA_COLORS"] = "Cam Nghệ (#FF9933) & Trắng Lụa (#F8F8FF)",
        ["CIV_INDIA_ATTIRE"] = "Khăn Turban to bản đính ngọc thái dương, áo lụa mỏng viền kim loại bạc filigree",
        ["CIV_INDIA_UNIT"] = "Xạ Thủ Phi Luân Chakram & Quang Huy Thần Tượng",
        ["CIV_INDIA_TRAIT"] = "Phi luân Chakram nảy mục tiêu liên hoàn, Chiến tượng nghi lễ phát quang tăng vọt sĩ khí",

        // 4. Zafaran
        ["CIV_PERSIA_NAME"] = "Hoàng Triều Bất Diệt Zafaran (Đế Quốc Sassanor)",
        ["CIV_PERSIA_EPITHET"] = "Hậu Duệ Quân Đoàn Bất Diệt",
        ["CIV_PERSIA_COLORS"] = "Tím Hoàng Gia (#66023C) & Vàng Nghệ (#DDAA00)",
        ["CIV_PERSIA_ATTIRE"] = "Khăn lụa tím bịt nửa mặt, áo dệt sợi vàng khoác ngoài giáp vảy kim hoàn, khiên đan liễu gai kiên cố",
        ["CIV_PERSIA_UNIT"] = "Kim Giáp Thiết Kỵ Cataphract & Quân Đoàn Bất Diệt",
        ["CIV_PERSIA_TRAIT"] = "Quân đoàn Bất Diệt tự hồi sinh khi tử trận; thiết kỵ cataphract xung phong phá hủy phòng tuyến",

        // 5. Viêm Hạo
        ["CIV_CHINA_NAME"] = "Thần Triều Viêm Hạo (Cửu Châu Thiên Triều)",
        ["CIV_CHINA_EPITHET"] = "Bậc Thầy Thiết Lũy & Hỏa Cơ",
        ["CIV_CHINA_COLORS"] = "Đỏ Thẫm Hoàng Gia (#8B0000) & Đen Then (#1C1C1C)",
        ["CIV_CHINA_ATTIRE"] = "Giáp phiến sắt vuông vức cổ phong, mũ thiết khôi cắm lông trĩ đỏ dài",
        ["CIV_CHINA_UNIT"] = "Hỏa Tiễn Thần Xa & Nỏ Cơ Trọng Giàn Công Thành",
        ["CIV_CHINA_TRAIT"] = "Điều động hỏa tiễn thần xa, xây thành lũy kiên cố và đồng hóa thành phố siêu tốc",

        // 6. Hinokuni
        ["CIV_JAPAN_NAME"] = "Thần Đạo Mạc Phủ Hinokuni (Thái Dương Kiếm Bang)",
        ["CIV_JAPAN_EPITHET"] = "Thần Kiếm Mặt Trời Thái Dương",
        ["CIV_JAPAN_COLORS"] = "Trắng Tuyết (#FFFAFA) & Đỏ Son Cổng Trời (#C8382B)",
        ["CIV_JAPAN_ATTIRE"] = "Giáp gỗ sơn mài truyền thống, mũ chiến Kabuto gắn sừng trăng khuyết lớn, cờ lệnh chữ nhật sau lưng",
        ["CIV_JAPAN_UNIT"] = "Huyết Thệ Kiếm Tướng & U Minh Âm Dương Sư",
        ["CIV_JAPAN_TRAIT"] = "Huyết Thệ Kiếm Tướng tăng sát thương cực đại khi cạn máu, Âm Dương Sư gọi sương mù che mắt đối phương",

        // 7. Gaorun
        ["CIV_KOREA_NAME"] = "Trùng Sơn Liên Vương Quốc Gaorun (Sơn Cương Triều)",
        ["CIV_KOREA_EPITHET"] = "Vệ Tướng Hiểm Địa Đỉnh Tuyết",
        ["CIV_KOREA_COLORS"] = "Xanh Lam Cobalt (#0047AB) & Nâu Da Thuộc (#7B3F00)",
        ["CIV_KOREA_ATTIRE"] = "Mũ chóp nỉ cắm hai lông chim ưng vểnh ngang, giáp vảy cá bó sát linh hoạt",
        ["CIV_KOREA_UNIT"] = "Phong Hoa Kỵ Xạ & Hỏa Lôi Xa Trận",
        ["CIV_KOREA_TRAIT"] = "Kỵ xạ dốc núi tầm xa vượt trội và hỏa lôi xa trận phóng bão tên hủy diệt diện rộng",

        // 8. Turgai
        ["CIV_MONGOLIA_NAME"] = "Đại Hãn Quốc Bạt Hãn Turgai (Hãn Quốc Lang Thần)",
        ["CIV_MONGOLIA_EPITHET"] = "Kỵ Sĩ Vòm Trời Xanh Vĩnh Hằng",
        ["CIV_MONGOLIA_COLORS"] = "Xanh Da Trời Thảo Nguyên (#4682B4) & Nâu Lông Sói (#5C4033)",
        ["CIV_MONGOLIA_ATTIRE"] = "Mũ lông thú tai cừu rủ, áo choàng da thú lót lông ấm, cung tên phức hợp phản khúc",
        ["CIV_MONGOLIA_UNIT"] = "Dạ Ưng Kỵ Xạ & Thiết Kỵ Thảo Nguyên",
        ["CIV_MONGOLIA_TRAIT"] = "Toàn quân kỵ binh cơ động; kỵ xạ vừa phi nước đại vừa bắn và rút lui tức thì sau công kích",

        // 9. Aethelon
        ["CIV_GREECE_NAME"] = "Liên Bang Đô Bang Aethelon (Thánh Bang Corinthia)",
        ["CIV_GREECE_EPITHET"] = "Phòng Tuyến Khiên Đồng Biển Ngọc",
        ["CIV_GREECE_COLORS"] = "Đồng Thau (#CD7F32) & Đỏ Cờ Chiến Trận (#990000)",
        ["CIV_GREECE_ATTIRE"] = "Mũ đồng thau hộ diện với mào lông ngựa đỏ uốn cong, khiên tròn đồng đúc phù hiệu thần tích, giáp đúc khối cơ ngực",
        ["CIV_GREECE_UNIT"] = "Thần Khiên Đồng Trận Hoplite & Kình Ngư Chiến Thuyền Tam Tầng",
        ["CIV_GREECE_TRAIT"] = "Trận đồ khiên đồng Hoplite phản đòn trực diện 100% sát thương cận chiến",

        // 10. Eldoria
        ["CIV_ENGLAND_NAME"] = "Phong Kiến Vương Quốc Eldoria (Lãnh Địa Rừng Xanh Albret)",
        ["CIV_ENGLAND_EPITHET"] = "Cung Thủ Rừng Cổ Thụ Greenwood",
        ["CIV_ENGLAND_COLORS"] = "Xanh Lá Cây Rừng (#228B22) & Nâu Bùn (#6E5334)",
        ["CIV_ENGLAND_ATTIRE"] = "Cung dài gỗ du vượt đầu người, áo chẽn nỉ thô, mũ chảo sắt tròn hộ đầu",
        ["CIV_ENGLAND_UNIT"] = "Xạ Thủ Trường Cung Eldoria & Thiết Kích Binh",
        ["CIV_ENGLAND_TRAIT"] = "Xạ thủ tầm bắn xa nhất sa bàn, khả năng dựng cọc nhọn bẫy kỵ binh xung trận",

        // 11. Valoisia
        ["CIV_FRANCE_NAME"] = "Hoàng Gia Đế Bang Valoisia (Vương Triều Bạch Diệp)",
        ["CIV_FRANCE_EPITHET"] = "Kỵ Sĩ Hoàng Gia Hoa Bạch Diệp",
        ["CIV_FRANCE_COLORS"] = "Xanh Dương Hoàng Gia (#4169E1) & Trắng Bạc (#E0E0E0)",
        ["CIV_FRANCE_ATTIRE"] = "Chiến mã phủ vải thêu hoa bách diệp chỉ vàng, hiệp sĩ mũ kín khe thở chữ thập, vác thương dài kỵ sĩ",
        ["CIV_FRANCE_UNIT"] = "Hoàng Gia Thiết Kỵ Hoa Thánh & Trọng Nỏ Phòng Tuyến",
        ["CIV_FRANCE_TRAIT"] = "Đòn húc kỵ sĩ làm vỡ tan đội hình địch và tạo khiên giảm thương cho đồng minh lân cận",

        // 12. Eisenreich
        ["CIV_GERMANY_NAME"] = "Thần Thánh Thiết Bang Eisenreich (Đại Công Quốc Teutonberg)",
        ["CIV_GERMANY_EPITHET"] = "Thiết Giáp Tiên Phong Thập Tự Đen",
        ["CIV_GERMANY_COLORS"] = "Đen Tuyền (#111111) & Trắng Tinh Khôi (#F5F5F5)",
        ["CIV_GERMANY_ATTIRE"] = "Áo choàng trắng thêu chữ thập đen trước ngực, mũ sắt gắn sừng hươu đồ sộ, toàn thân bọc giáp phiến kiên cố",
        ["CIV_GERMANY_UNIT"] = "Trảm Kiếm Sĩ Đại Kiếm Zweihander & Thiết Kỵ Thập Tự Đen",
        ["CIV_GERMANY_TRAIT"] = "Trảm kiếm sĩ Zweihander chém quét lan 3 ô, chuyên phá vỡ các phòng tuyến khiên giáo",

        // 13. Castilia
        ["CIV_SPAIN_NAME"] = "Kim Kiếm Vương Triều Castilia (Thánh Đình Aragonis)",
        ["CIV_SPAIN_EPITHET"] = "Phương Trận Kim Giáp Bất Bại",
        ["CIV_SPAIN_COLORS"] = "Vàng Kim (#DAA520) & Đỏ Tươi (#C70039)",
        ["CIV_SPAIN_ATTIRE"] = "Mũ vành nhọn hai đầu phản quang, giáp ngực thép tôi luyện, kiếm thép dài tôi dầu sắc bén",
        ["CIV_SPAIN_UNIT"] = "Kim Giáp Phương Trận Tercio & Kỵ Binh Viễn Chinh Castilia",
        ["CIV_SPAIN_TRAIT"] = "Đội hình Tercio phối hợp trường giáo kiên cố miễn nhiễm hoàn toàn các đòn húc kỵ binh",

        // 14. Borislav
        ["CIV_SLAVS_NAME"] = "Cổ Minh Thần Mộc Borislav (Thần Mộc Đại Bang)",
        ["CIV_SLAVS_EPITHET"] = "Chiến Binh Cổ Thụ Lôi Thần",
        ["CIV_SLAVS_COLORS"] = "Đỏ Thêu Dân Gian (#C8102E) & Xám Da Sói (#696969)",
        ["CIV_SLAVS_ATTIRE"] = "Áo lanh thêu hoa văn đỏ linh thiêng, khoác da sói xám, râu tết chuỗi hạt gỗ hộ mệnh",
        ["CIV_SLAVS_UNIT"] = "Sâm Lâm Thiết Dũng Vityaz & Tế Lễ Lôi Thần Perun",
        ["CIV_SLAVS_TRAIT"] = "Chiến binh Vityaz hồi máu gần bìa rừng, miễn nhiễm làm chậm trong bão tuyết và băng giá",

        // 15. Krumak
        ["CIV_BULGARS_NAME"] = "Hãn Bang Huyết Kỳ Krumak (Đại Hãn Quốc Tangrad)",
        ["CIV_BULGARS_EPITHET"] = "Kỵ Sĩ Ngọn Cờ Thiêng Tangrad",
        ["CIV_BULGARS_COLORS"] = "Đỏ Huyết Dụ (#8A0303) & Đen Than Củi (#232B2B)",
        ["CIV_BULGARS_ATTIRE"] = "Mũ sắt chóp vuốt cong đính đuôi ngựa đen, cọc cờ hiệu thiêng thần Tangrad, giáp vảy bọc da lộn",
        ["CIV_BULGARS_UNIT"] = "Trọng Kỵ Quý Tộc Boyar & Huyết Thệ Cuồng Binh",
        ["CIV_BULGARS_TRAIT"] = "Kỵ binh Boyar cướp bóc tài nguyên từ xác tướng lĩnh địch; bộ binh kích hoạt Huyết Thệ cuồng bạo khi nguy cấp",

        // 16. Volskya
        ["CIV_RUS_NAME"] = "Bắc Cương Đại Lãnh Địa Volskya (Vương Bang Băng Hồ)",
        ["CIV_RUS_EPITHET"] = "Vệ Binh Băng Giá Dòng Dniepran",
        ["CIV_RUS_COLORS"] = "Đỏ Rượu Chát (#800020) & Trắng Tuyết (#FFFAFA)",
        ["CIV_RUS_ATTIRE"] = "Áo khoác da gấu dày viền lông thú bắc cực, mũ chóp nhọn lưới xích che kín cằm, vác đại phủ hai lưỡi Bardiche",
        ["CIV_RUS_UNIT"] = "Bắc Cương Thân Binh Druzhina & Đại Phủ Băng Hàn Bardiche",
        ["CIV_RUS_TRAIT"] = "Thân binh Druzhina nhận lượng phòng ngự cực đại trong đầm lầy hoặc bão tuyết",

        // 17. Sarmatia
        ["CIV_POLAND_NAME"] = "Vũ Dực Vương Triều Sarmatia (Đại Vương Quốc Lechia)",
        ["CIV_POLAND_EPITHET"] = "Thần Ưng Cánh Thép Bão Táp",
        ["CIV_POLAND_COLORS"] = "Đỏ Thắm Hoàng Gia (#DC143C) & Trắng Lông Vũ (#FFFFFF)",
        ["CIV_POLAND_ATTIRE"] = "Khung cánh lông vũ đại bàng cong vút sau lưng giáp ngực thép bóng, thương dài gắn cờ lụa hai màu",
        ["CIV_POLAND_UNIT"] = "Thần Dực Thiết Kỵ Winged Husaria & Giáp Xích Thiết Kỵ Pancerni",
        ["CIV_POLAND_TRAIT"] = "Thiết kỵ Husaria phi nước đại tạo âm thanh rít gió uy hiếp, khiến quân địch hoảng loạn suy giảm sĩ khí",

        // 18. Tlalokan
        ["CIV_MESO_NAME"] = "Thần Vũ Vương Triều Tlalokan (Thánh Địa Quetzala)",
        ["CIV_MESO_EPITHET"] = "Dũng Sĩ Báo Đốm Vũ Xà Thần",
        ["CIV_MESO_COLORS"] = "Đỏ Đất Nung (#B22222) & Xanh Lông Chim Thần (#00A86B)",
        ["CIV_MESO_ATTIRE"] = "Mũ lông chim ưng xòe tròn lớn, mình vẽ hoa văn thần thánh, áo giáp da báo đốm dũng mãnh",
        ["CIV_MESO_UNIT"] = "Huyền Báo Thần Dũng Jaguar & Phi Lao Thần Nhật Atlatl",
        ["CIV_MESO_TRAIT"] = "Huyền Báo Thần Dũng di chuyển nhanh ngang kỵ binh, không chịu hình phạt tốc độ khi vượt địa hình hiểm trở",

        // Stages
        ["STAGE_1_TITLE"] = "Ải 1: Phù Sa Thần Tích (Bách Thần & Phù Sa)",
        ["STAGE_1_DESC"] = "Khai mở vương nghiệp tại châu thổ Đại Hà Xích Giang. Đắp đê chế ngự lũ lụt, dựng bẫy cọc lòng sông, thờ phụng thủy thần và rèn vũ khí đồng sơ kỳ để tạo dựng lãnh thổ khởi nguyên.",
        ["STAGE_1_OBJ"] = "Khai hoang 3 đại bãi phù sa màu mỡ, lập tiền đồn bến sông & huấn luyện 15 dân binh tiên phong",
        ["STAGE_1_ENEMY"] = "Hắc Thủy Man Tộc Ven Sông",
        ["STAGE_1_REWARD"] = "Lĩnh hội: Kỹ Thuật Đúc Đồng Thần Tích & Xạ Thủ Nỏ Máy Uyên Châu",

        ["STAGE_2_TITLE"] = "Ải 2: Vó Ngựa & Hỏa Cơ (Thiết Kỷ & Thiên Mệnh)",
        ["STAGE_2_DESC"] = "Hành quân tiến về thảo nguyên bao la. Chống trả những đợt bôn tập thần tốc của kỵ xạ du mục và công phá trận địa xe bắn hỏa tiễn, nỏ cơ giới khổng lồ của các triều đình giáp sắt.",
        ["STAGE_2_OBJ"] = "Đập tan 3 cỗ Hỏa Lôi Xa Trận, bẻ gãy đợt kỵ xạ bôn tập & chiếm giữ ải khẩu thảo nguyên",
        ["STAGE_2_ENEMY"] = "Thiết Kỵ Hãn Quốc & Quân Cơ Giới Phương Bắc",
        ["STAGE_2_REWARD"] = "Lĩnh hội: Chiến Xa Hỏa Pháo & Trận Đồ Kỵ Xạ Du Mục",

        ["STAGE_3_TITLE"] = "Ải 3: Trọng Thép & Phương Trận (Thánh Huy & Trọng Thép)",
        ["STAGE_3_DESC"] = "Vượt Biển Ngọc tiến vào các lãnh địa phương Tây. Đối diện tường đồng lũy thép của trận đồ giáo dài Phalanx kiên cố và phá vỡ cú húc long trời lở đất của các đoàn hiệp sĩ giáp sắt hoàng gia.",
        ["STAGE_3_OBJ"] = "Xuyên phá phương trận Hoplite Phalanx, cản phá đòn thiết kỵ húc trận & hạ thành trì cự thạch",
        ["STAGE_3_ENEMY"] = "Quân Đoàn Thánh Huy & Thiết Giáp Hiệp Sĩ",
        ["STAGE_3_REWARD"] = "Lĩnh hội: Kỹ Thuật Trọng Giáp Toàn Thân & Phương Trận Trường Thương",

        ["STAGE_4_TITLE"] = "Ải 4: Tuyết Phủ Rừng Thiêng (Hoang Thần & Băng Lãnh)",
        ["STAGE_4_DESC"] = "Tiến sâu vào rừng già cổ thụ và đầm lầy băng giá Bắc Cương. Chống chọi cơn bão tuyết gầm thét, càn quét sào huyệt dũng sĩ man tộc tôn sùng cổ thần sấm sét và kỵ sĩ mang cánh đại bàng.",
        ["STAGE_4_OBJ"] = "Vượt bão tuyết lãnh nguyên, đánh bại đại dũng sĩ Vityaz & tiêu diệt chiến binh cuồng nộ",
        ["STAGE_4_ENEMY"] = "Bộ Tộc Băng Lãnh & Cổ Thần Tế Lễ",
        ["STAGE_4_REWARD"] = "Lĩnh hội: Khí Chất Băng Hàn Kháng Bão & Dũng Khí Cuồng Nộ Huyết Thệ",

        ["STAGE_5_TITLE"] = "Ải 5: Vạn Quốc Tranh Hùng (Đại Chiến Đỉnh Phong)",
        ["STAGE_5_DESC"] = "Hội chiến vĩ đại tại trung tâm thế giới. Mười tám vương triều từ cả bốn khối đại lục cùng dàn quân quyết đấu; đánh bại liên quân các đại đế, hưng kiến Kỳ Đài Vạn Thế và định đoạt thiên mệnh ngàn năm.",
        ["STAGE_5_OBJ"] = "Đánh bại tam đại liên minh đế quốc hoặc hoàn thành Kỳ Đài Vạn Bang Trấn Quốc",
        ["STAGE_5_ENEMY"] = "Tứ Đại Khối Văn Minh Hợp Kháng Đồng Minh",
        ["STAGE_5_REWARD"] = "Tôn hiệu: ĐẠI ĐẾ VẠN QUỐC & Thống Nhất Bát Hoang Thiên Hạ",

        // Skirmish Settings
        ["SKIRMISH_MAP_SIZE"] = "Quy mô sa bàn cương vực",
        ["SKIRMISH_BIOME"] = "Thổ nhưỡng & Địa mạo non sông",
        ["SKIRMISH_RIVALS"] = "Số lượng đế bang kình địch",
        ["SKIRMISH_VICTORY"] = "Đại định quy ước định thắng",
        ["SKIRMISH_DIFFICULTY"] = "Độ khó quân cơ địch quốc",
        ["MAP_SMALL"] = "Tiểu Cương Vực (32x32) - Giao phong thần tốc",
        ["MAP_MEDIUM"] = "Trung Sa Bàn (64x64) - Dàn trận thao lược",
        ["MAP_LARGE"] = "Đại Lục Bao La (128x128) - Huyết chiến vạn bang",
        ["BIOME_RED_RIVER"] = "Châu thổ Đại Hà (Lưu vực phù sa màu mỡ, bẫy cọc ngầm lòng sông)",
        ["BIOME_JUNGLE"] = "U Tịch Rừng Già (Sương mù phục kích, đầm lầy cạm bẫy)",
        ["BIOME_HIGHLANDS"] = "Hiểm Địa Cao Nguyên (Khoáng thạch trù phú, quan ải cheo leo)",
        ["BIOME_STEPPE"] = "Đại Thảo Nguyên Cương Vực (Bình nguyên gió lộng, kỵ binh bôn tập)",
        ["BIOME_TAIGA"] = "Lãnh Nguyên Băng Giá (Bão tuyết thét gào, hành quân gian nan)",
        ["BIOME_MEDITERRANEAN"] = "Biển Ngọc Phong Vân (Cảng khẩu kiên cố, chiến hạm tung hoành)",
        ["RIVALS_2"] = "Song Hùng Đoạt Bá (2 Thế Lực)",
        ["RIVALS_4"] = "Tứ Phương Tranh Phong (4 Thế Lực)",
        ["RIVALS_6"] = "Lục Cương Phân Huyết (6 Thế Lực)",
        ["RIVALS_8"] = "Bát Hoang Hỗn Chiến (8 Thế Lực)",
        ["VICTORY_CONQUEST"] = "Bá Nghiệp Thôn Tính (Tiêu diệt toàn quân)",
        ["VICTORY_CULTURE"] = "Kỳ Đài Vạn Thế (Thần Khí Trấn Quốc)",
        ["VICTORY_REGICIDE"] = "Trảm Tướng Đoạt Kỳ (Diệt thủ lĩnh địch)",
        ["DIFF_EASY"] = "Sơ Khởi (Tập sự quân cơ)",
        ["DIFF_NORMAL"] = "Hùng Binh (Thống soái thao lược)",
        ["DIFF_HARD"] = "Thiết Huyết (Kiêu hùng mưu độc)",
        ["DIFF_LEGEND"] = "Thiên Mệnh Thần Võ (Đại đế xuất thế)",

        // Training
        ["TRAINING_SELECT"] = "Chọn Trận Đồ",
        ["TRAINING_SELECTED"] = "Đã Chọn Trận",
        ["TRAINING_TUTORIAL_NAME"] = "Binh Pháp Điểm Binh Diễn Võ",
        ["TRAINING_SANDBOX_NAME"] = "Sa Bàn Khảo Nghiệm Chiến Binh",

        // Settings
        ["SETTINGS_TITLE"] = "CÀI ĐẶT HỆ THỐNG",
        ["SETTINGS_TAB_AUDIO"] = "🔊 Âm Thanh",
        ["SETTINGS_TAB_VIDEO"] = "🖥 Hiển Thị",
        ["SETTINGS_TAB_GAMEPLAY"] = "🌐 Ngôn Ngữ",
        ["SETTINGS_MASTER_VOL"] = "Âm Lượng Tổng",
        ["SETTINGS_SFX_VOL"] = "Hiệu Ứng Âm Thanh",
        ["SETTINGS_MUSIC_VOL"] = "Nhạc Nền",
        ["SETTINGS_FULLSCREEN"] = "Toàn Màn Hình",
        ["SETTINGS_VSYNC"] = "Đồng Bộ Dọc (VSync)",
        ["SETTINGS_LANGUAGE"] = "Ngôn Ngữ",
        ["SETTINGS_BTN_CLOSE"] = "✖ Đóng",

        // Display Settings
        ["SETTINGS_WINDOW_MODE"] = "Chế Độ Cửa Sổ",
        ["SETTINGS_RESOLUTION"] = "Độ Phân Giải",
        ["SETTINGS_VSYNC_LABEL"] = "Đồng Bộ Dọc (VSync)",
        ["SETTINGS_MAX_FPS"] = "Khung Hình Tối Đa (FPS)",
        ["SETTINGS_MODE_WINDOWED"] = "Cửa Sổ",
        ["SETTINGS_MODE_BORDERLESS"] = "Không Viền",
        ["SETTINGS_MODE_FULLSCREEN"] = "Toàn Màn Hình",
        ["SETTINGS_MODE_EXCLUSIVE"] = "Toàn Màn Hình Độc Quyền",
        ["SETTINGS_VSYNC_DISABLED"] = "Tắt",
        ["SETTINGS_VSYNC_ENABLED"] = "Bật",
        ["SETTINGS_VSYNC_ADAPTIVE"] = "Thích Ứng",
        ["SETTINGS_FPS_UNLIMITED"] = "Không Giới Hạn",
        ["SETTINGS_UI_SCALE"] = "Tỷ Lệ Giao Diện",
        ["SETTINGS_BTN_SAVE"] = "💾 Lưu & Áp Dụng",
        ["SETTINGS_STATUS_SAVED"] = "Đã lưu & áp dụng cài đặt!",
        ["SETTINGS_STATUS_SAVED_VI"] = "Đã lưu & áp dụng cài đặt!"
    };
}
