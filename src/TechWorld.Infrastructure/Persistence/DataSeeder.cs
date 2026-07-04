using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechWorld.Domain.Entities;
using TechWorld.Infrastructure.Identity;

namespace TechWorld.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        await SeedAdminAsync(userManager, context);
        await SeedProductsAsync(context);
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        const string adminEmail = "admin@techworld.com";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            DisplayName = "Admin",
            IsAdmin = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Falha ao criar admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        var profile = UserProfile.Create(admin.Id, "Admin");
        await context.UserProfiles.AddAsync(profile);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        var products = BuildProducts();

        var existingNames = await context.Products
            .Select(p => p.Name)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        var toAdd = products.Where(p => !existingSet.Contains(p.Name)).ToList();
        if (toAdd.Count > 0)
        {
            await context.Products.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
        }

        await PatchProductImagesAsync(context);
    }

    // Fills imageUrl for products that still have null (existing or newly added)
    private static async Task PatchProductImagesAsync(ApplicationDbContext context)
    {
        var images = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Smartphones
            ["Samsung Galaxy S25 Ultra"]       = "https://images.unsplash.com/photo-1610945415814-7f8e8d96cd3b?w=600&auto=format&fit=crop&q=80",
            ["iPhone 16 Pro Max"]              = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=600&auto=format&fit=crop&q=80",
            ["Motorola Edge 50 Pro"]           = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=600&auto=format&fit=crop&q=80",
            ["Xiaomi Redmi Note 14 Pro"]       = "https://images.unsplash.com/photo-1574944985070-8f3ebc6b79d2?w=600&auto=format&fit=crop&q=80",
            ["Google Pixel 9 Pro"]             = "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=600&auto=format&fit=crop&q=80",
            ["Samsung Galaxy A55 5G"]          = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=600&auto=format&fit=crop&q=80",
            ["OnePlus 12"]                     = "https://images.unsplash.com/photo-1556656793-08538906a9f8?w=600&auto=format&fit=crop&q=80",
            // Computadores
            ["MacBook Air 15\" M3"]            = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&auto=format&fit=crop&q=80",
            ["Notebook Dell Inspiron 15 i5"]   = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600&auto=format&fit=crop&q=80",
            ["Desktop Gamer Acer Predator Orion 7000"] = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
            ["Chromebook Asus CX1500"]         = "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=600&auto=format&fit=crop&q=80",
            // Gaming
            ["Teclado Mecânico HyperX Alloy Origins"] = "https://images.unsplash.com/photo-1541140532154-b174497b51a6?w=600&auto=format&fit=crop&q=80",
            ["Mouse Logitech G Pro X Superlight 2"]   = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=600&auto=format&fit=crop&q=80",
            ["Headset SteelSeries Arctis Nova Pro"]   = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=80",
            ["Monitor Gamer Samsung Odyssey G5 27\""] = "https://images.unsplash.com/photo-1547394765-185e1e68f34e?w=600&auto=format&fit=crop&q=80",
            ["Cadeira Gamer Corsair TC500 Luxe"]      = "https://images.unsplash.com/photo-1598550476439-6847785fcea6?w=600&auto=format&fit=crop&q=80",
            ["Controle DualSense PS5"]         = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
            ["Controle Xbox Series X/S"]       = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
            ["Nintendo Switch Pro Controller"] = "https://images.unsplash.com/photo-1585987561578-e46f5e3db2a1?w=600&auto=format&fit=crop&q=80",
            ["Webcam Logitech C922 Pro"]        = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&auto=format&fit=crop&q=80",
            ["Mousepad Gamer XL Redragon P003"] = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
            // Consoles
            ["PlayStation 5 Slim"]             = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
            ["Xbox Series X"]                  = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
            ["Nintendo Switch OLED"]           = "https://images.unsplash.com/photo-1585987561578-e46f5e3db2a1?w=600&auto=format&fit=crop&q=80",
            ["PlayStation 5 Digital Edition"]  = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
            ["Xbox Series S"]                  = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
            ["Steam Deck OLED"]                = "https://images.unsplash.com/photo-1592899677977-9c10002761e8?w=600&auto=format&fit=crop&q=80",
            // Componentes
            ["Placa de Vídeo ASUS ROG RTX 4070 Ti Super"] = "https://images.unsplash.com/photo-1591405351990-4726e331f141?w=600&auto=format&fit=crop&q=80",
            ["Processador Intel Core i9-14900K"]          = "https://images.unsplash.com/photo-1591267990532-e5bdb1b0cae8?w=600&auto=format&fit=crop&q=80",
            ["Memória RAM Corsair Vengeance 32 GB DDR5"]  = "https://images.unsplash.com/photo-1562976540-1502c2145851?w=600&auto=format&fit=crop&q=80",
            ["SSD Samsung 990 Pro 2 TB"]                  = "https://images.unsplash.com/photo-1597852074816-d933c7d2b988?w=600&auto=format&fit=crop&q=80",
            ["Placa-mãe ASUS ROG Strix Z790-E Gaming"]    = "https://images.unsplash.com/photo-1518770660439-4636190af475?w=600&auto=format&fit=crop&q=80",
            ["Cooler CPU Noctua NH-D15 G2"]               = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=600&auto=format&fit=crop&q=80",
            ["AMD Ryzen 9 7950X"]                         = "https://images.unsplash.com/photo-1591267990532-e5bdb1b0cae8?w=600&auto=format&fit=crop&q=80",
            ["Placa de Vídeo XFX RX 7900 XTX"]           = "https://images.unsplash.com/photo-1591405351990-4726e331f141?w=600&auto=format&fit=crop&q=80",
            ["Fonte Corsair RM1000x 1000W"]               = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=600&auto=format&fit=crop&q=80",
            ["Gabinete Lian Li PC-O11 Dynamic EVO"]       = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
        };

        foreach (var (name, url) in images)
        {
            await context.Products
                .Where(p => p.Name == name && (p.ImageUrl == null || p.ImageUrl == ""))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ImageUrl, url));
        }
    }

    private static List<Product> BuildProducts()
    {
        var list = new List<Product>();

        // ── Smartphones ──────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "Samsung Galaxy S25 Ultra",
                price: 7299m,
                category: "Smartphones",
                description: "O Galaxy S25 Ultra redefine o que um smartphone pode fazer, com S Pen integrada, câmera de 200 MP e processador Snapdragon 8 Elite.",
                imageUrl: "https://images.unsplash.com/photo-1610945415814-7f8e8d96cd3b?w=600&auto=format&fit=crop&q=80",
                stock: 40,
                discountPercentage: 15,
                brand: "Samsung"),
            ("Processador", "Snapdragon 8 Elite"),
            ("RAM", "12 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "200 MP"),
            ("Bateria", "5.000 mAh"),
            ("Tela", "6,8\" QHD+ AMOLED 120Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "iPhone 16 Pro Max",
                price: 9499m,
                category: "Smartphones",
                description: "O iPhone 16 Pro Max apresenta o chip A18 Pro, câmera Fusion de 48 MP e a maior tela Pro já feita com titânio de grau aeroespacial.",
                imageUrl: "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=600&auto=format&fit=crop&q=80",
                stock: 25,
                discountPercentage: 10,
                brand: "Apple"),
            ("Processador", "Apple A18 Pro"),
            ("RAM", "8 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "48 MP"),
            ("Bateria", "4.685 mAh"),
            ("Tela", "6,9\" Super Retina XDR OLED 120Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Motorola Edge 50 Pro",
                price: 2799m,
                category: "Smartphones",
                description: "Motorola Edge 50 Pro com carregamento TurboPower de 125W, câmera tripla Pantone e tela pOLED de 144Hz.",
                imageUrl: "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=600&auto=format&fit=crop&q=80",
                stock: 60,
                discountPercentage: 0,
                brand: "Motorola"),
            ("Processador", "Snapdragon 7s Gen 3"),
            ("RAM", "12 GB"),
            ("Armazenamento", "512 GB"),
            ("Câmera Principal", "50 MP"),
            ("Bateria", "4.500 mAh + TurboPower 125W"),
            ("Tela", "6,7\" pOLED 144Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Xiaomi Redmi Note 14 Pro",
                price: 1999m,
                category: "Smartphones",
                description: "Excelente custo-benefício com câmera de 200 MP, tela AMOLED de 120Hz e carregamento rápido de 67W.",
                imageUrl: "https://images.unsplash.com/photo-1574944985070-8f3ebc6b79d2?w=600&auto=format&fit=crop&q=80",
                stock: 80,
                discountPercentage: 20,
                brand: "Xiaomi"),
            ("Processador", "Snapdragon 7s Gen 3"),
            ("RAM", "8 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "200 MP"),
            ("Bateria", "5.500 mAh"),
            ("Tela", "6,67\" AMOLED 120Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Google Pixel 9 Pro",
                price: 6999m,
                category: "Smartphones",
                description: "Google Pixel 9 Pro com câmera computacional de 50 MP, chip Tensor G4 e 7 anos de atualizações garantidas.",
                imageUrl: "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=600&auto=format&fit=crop&q=80",
                stock: 30,
                discountPercentage: 5,
                brand: "Google"),
            ("Processador", "Google Tensor G4"),
            ("RAM", "16 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "50 MP (tripla)"),
            ("Bateria", "5.060 mAh"),
            ("Tela", "6,3\" LTPO OLED 120Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Samsung Galaxy A55 5G",
                price: 2199m,
                category: "Smartphones",
                description: "Galaxy A55 5G com design premium em vidro Corning Gorilla Glass Victus+, câmera tripla de 50 MP e IP67.",
                imageUrl: "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=600&auto=format&fit=crop&q=80",
                stock: 90,
                discountPercentage: 10,
                brand: "Samsung"),
            ("Processador", "Exynos 1480"),
            ("RAM", "8 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "50 MP"),
            ("Bateria", "5.000 mAh"),
            ("Tela", "6,6\" Super AMOLED 120Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "OnePlus 12",
                price: 4799m,
                category: "Smartphones",
                description: "OnePlus 12 com Snapdragon 8 Gen 3, câmera Hasselblad de 50 MP e carregamento SUPERVOOC de 100W.",
                imageUrl: "https://images.unsplash.com/photo-1556656793-08538906a9f8?w=600&auto=format&fit=crop&q=80",
                stock: 45,
                discountPercentage: 0,
                brand: "OnePlus"),
            ("Processador", "Snapdragon 8 Gen 3"),
            ("RAM", "12 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "50 MP Hasselblad"),
            ("Bateria", "5.400 mAh + SUPERVOOC 100W"),
            ("Tela", "6,82\" LTPO3 AMOLED 120Hz")));

        // ── Computadores ─────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "MacBook Air 15\" M3",
                price: 12999m,
                category: "Computadores",
                description: "Incrivelmente fino e leve com o chip M3 da Apple. Até 18 horas de bateria, tela Liquid Retina de 15,3\" e câmera FaceTime 1080p.",
                imageUrl: "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&auto=format&fit=crop&q=80",
                stock: 18,
                discountPercentage: 0,
                brand: "Apple"),
            ("Processador", "Apple M3 (8 núcleos CPU)"),
            ("GPU", "10 núcleos GPU"),
            ("RAM", "16 GB"),
            ("Armazenamento", "512 GB SSD"),
            ("Tela", "15,3\" Liquid Retina 2.880 x 1.864"),
            ("Bateria", "Até 18 horas")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Notebook Dell Inspiron 15 i5",
                price: 3899m,
                category: "Computadores",
                description: "Notebook Dell com processador Intel Core i5 de 13ª geração, tela Full HD e SSD de 512 GB para uso do dia a dia.",
                imageUrl: "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600&auto=format&fit=crop&q=80",
                stock: 35,
                discountPercentage: 10,
                brand: "Dell"),
            ("Processador", "Intel Core i5-1335U"),
            ("RAM", "16 GB DDR4"),
            ("Armazenamento", "512 GB SSD"),
            ("Tela", "15,6\" Full HD IPS"),
            ("Sistema Operacional", "Windows 11 Home"),
            ("Peso", "1,76 kg")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Desktop Gamer Acer Predator Orion 7000",
                price: 15999m,
                category: "Computadores",
                description: "Potência extrema com Intel Core i9, RTX 4090 e refrigeração líquida para dominar qualquer jogo em 4K.",
                imageUrl: "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
                stock: 8,
                discountPercentage: 0,
                brand: "Acer"),
            ("Processador", "Intel Core i9-14900K"),
            ("Placa de Vídeo", "NVIDIA RTX 4090 24 GB"),
            ("RAM", "64 GB DDR5 5600 MHz"),
            ("Armazenamento", "2 TB SSD NVMe"),
            ("Refrigeração", "Líquida 360mm"),
            ("Sistema Operacional", "Windows 11 Pro")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Chromebook Asus CX1500",
                price: 1599m,
                category: "Computadores",
                description: "Chromebook leve e acessível para navegação, streaming e tarefas escolares com bateria de longa duração.",
                imageUrl: "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=600&auto=format&fit=crop&q=80",
                stock: 50,
                discountPercentage: 5,
                brand: "Asus"),
            ("Processador", "Intel Celeron N4500"),
            ("RAM", "8 GB LPDDR4x"),
            ("Armazenamento", "64 GB eMMC"),
            ("Tela", "15,6\" Full HD IPS"),
            ("Bateria", "Até 10 horas"),
            ("Sistema Operacional", "Chrome OS")));

        // ── Gaming ───────────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "Teclado Mecânico HyperX Alloy Origins",
                price: 599m,
                category: "Gaming",
                description: "Teclado mecânico compacto com switches HyperX Red lineares, iluminação RGB e corpo em alumínio aeroespacial.",
                imageUrl: "https://images.unsplash.com/photo-1541140532154-b174497b51a6?w=600&auto=format&fit=crop&q=80",
                stock: 70,
                discountPercentage: 15,
                brand: "HyperX"),
            ("Switch", "HyperX Red (linear)"),
            ("Layout", "TKL (87 teclas)"),
            ("Conexão", "USB-C"),
            ("Iluminação", "RGB per-key"),
            ("Anti-ghosting", "Rollover completo N-key"),
            ("Compatibilidade", "Windows / Mac")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Mouse Logitech G Pro X Superlight 2",
                price: 799m,
                category: "Gaming",
                description: "Mouse sem fio ultraleve de apenas 60g com sensor Hero 2 de 32.000 DPI para gamers profissionais.",
                imageUrl: "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=600&auto=format&fit=crop&q=80",
                stock: 55,
                discountPercentage: 0,
                brand: "Logitech"),
            ("Sensor", "HERO 2 — 32.000 DPI"),
            ("Peso", "60 g"),
            ("Conexão", "LIGHTSPEED sem fio + USB-A"),
            ("Bateria", "Até 95 horas"),
            ("Botões", "5 programáveis"),
            ("Polling rate", "2.000 Hz")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Headset SteelSeries Arctis Nova Pro",
                price: 1499m,
                category: "Gaming",
                description: "Headset premium com cancelamento ativo de ruído, sistema de bateria hot-swap e som hi-fi para PC e console.",
                imageUrl: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=80",
                stock: 30,
                discountPercentage: 10,
                brand: "SteelSeries"),
            ("Driver", "40 mm neodímio"),
            ("Resposta de frequência", "10 Hz – 40 kHz"),
            ("Cancelamento de Ruído", "ANC ativo"),
            ("Conexão", "USB + 3,5mm + Bluetooth"),
            ("Bateria", "Hot-swap (bateria dupla)"),
            ("Plataforma", "PC, PS5, Xbox, Mobile")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Monitor Gamer Samsung Odyssey G5 27\"",
                price: 2199m,
                category: "Gaming",
                description: "Monitor curvo 1000R com resolução QHD, 165Hz e 1ms para uma experiência imersiva em jogos.",
                imageUrl: "https://images.unsplash.com/photo-1547394765-185e1e68f34e?w=600&auto=format&fit=crop&q=80",
                stock: 22,
                discountPercentage: 0,
                brand: "Samsung"),
            ("Painel", "VA Curvo 1000R"),
            ("Resolução", "2.560 x 1.440 (QHD)"),
            ("Taxa de Atualização", "165 Hz"),
            ("Tempo de Resposta", "1 ms"),
            ("HDR", "HDR10"),
            ("Sync", "FreeSync Premium")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Cadeira Gamer Corsair TC500 Luxe",
                price: 3499m,
                category: "Gaming",
                description: "Cadeira gamer premium com couro genuíno italiano, apoio lombar ajustável e construção de alumínio.",
                imageUrl: "https://images.unsplash.com/photo-1598550476439-6847785fcea6?w=600&auto=format&fit=crop&q=80",
                stock: 15,
                discountPercentage: 0,
                brand: "Corsair"),
            ("Material", "Couro genuíno italiano"),
            ("Inclinação", "Até 165°"),
            ("Altura do assento", "40 – 51 cm"),
            ("Largura do assento", "56 cm"),
            ("Capacidade", "até 120 kg"),
            ("Base", "Alumínio polido")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Controle DualSense PS5",
                price: 499m,
                category: "Gaming",
                description: "Controle do PS5 com feedback háptico, gatilhos adaptáveis e microfone embutido para uma experiência imersiva.",
                imageUrl: "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
                stock: 60,
                discountPercentage: 0,
                brand: "Sony"),
            ("Conexão", "USB-C + Bluetooth 5.1"),
            ("Bateria", "1.560 mAh"),
            ("Feedback", "Háptico + gatilhos adaptáveis"),
            ("Microfone", "Embutido"),
            ("Touchpad", "Capacitivo"),
            ("Compatibilidade", "PS5 + PC")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Controle Xbox Series X/S",
                price: 449m,
                category: "Gaming",
                description: "Controle Xbox com textura antiderrapante, botão Share, entrada P2 e compatibilidade com Xbox, PC e mobile.",
                imageUrl: "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
                stock: 75,
                discountPercentage: 0,
                brand: "Microsoft"),
            ("Conexão", "USB-C + Bluetooth + Xbox Wireless"),
            ("Bateria", "AA (incluída) ou bateria recarregável"),
            ("Botão Share", "Sim"),
            ("Entrada P2", "3,5 mm"),
            ("Compatibilidade", "Xbox Series X/S, Xbox One, PC, Mobile"),
            ("Textura", "Antiderrapante")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Nintendo Switch Pro Controller",
                price: 599m,
                category: "Gaming",
                description: "Controle profissional do Nintendo Switch com NFC para amiibo, giroscópio e bateria de até 40 horas.",
                imageUrl: "https://images.unsplash.com/photo-1585987561578-e46f5e3db2a1?w=600&auto=format&fit=crop&q=80",
                stock: 40,
                discountPercentage: 0,
                brand: "Nintendo"),
            ("Conexão", "Bluetooth + USB-C"),
            ("Bateria", "1.300 mAh — até 40 horas"),
            ("Giroscópio", "6 eixos"),
            ("NFC", "Sim (suporte a amiibo)"),
            ("Rumble", "HD Rumble"),
            ("Compatibilidade", "Nintendo Switch / Switch Lite / Switch OLED")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Webcam Logitech C922 Pro",
                price: 449m,
                category: "Gaming",
                description: "Webcam Full HD 1080p/30fps ou 720p/60fps com remoção de fundo por software e microfone estéreo integrado.",
                imageUrl: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&auto=format&fit=crop&q=80",
                stock: 50,
                discountPercentage: 5,
                brand: "Logitech"),
            ("Resolução", "1080p/30fps ou 720p/60fps"),
            ("Campo de Visão", "78°"),
            ("Microfone", "Estéreo integrado"),
            ("Foco", "Automático"),
            ("Conexão", "USB-A"),
            ("Compatibilidade", "Windows, macOS, Chrome OS")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Mousepad Gamer XL Redragon P003",
                price: 129m,
                category: "Gaming",
                description: "Mousepad extra grande 900x400mm com base antiderrapante, bordas costuradas e superfície otimizada para alta precisão.",
                imageUrl: "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
                stock: 120,
                discountPercentage: 0,
                brand: "Redragon"),
            ("Dimensões", "900 x 400 x 3 mm"),
            ("Superfície", "Microfibra de alta precisão"),
            ("Base", "Borracha antiderrapante"),
            ("Bordas", "Costuradas"),
            ("Peso", "450 g"),
            ("Indicado para", "Mouse óptico e laser")));

        // ── Consoles ─────────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "PlayStation 5 Slim",
                price: 3999m,
                category: "Consoles",
                description: "O PS5 Slim chega 30% menor que o original com leitor de disco removível, 1 TB de SSD e suporte a jogos em 4K/120fps.",
                imageUrl: "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
                stock: 20,
                discountPercentage: 0,
                brand: "Sony"),
            ("CPU", "AMD Zen 2 — 3,5 GHz (8 núcleos)"),
            ("GPU", "AMD RDNA 2 — 10,3 TFLOPS"),
            ("RAM", "16 GB GDDR6"),
            ("Armazenamento", "1 TB SSD NVMe"),
            ("Resolução", "Até 8K"),
            ("Ray Tracing", "Hardware dedicado")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Xbox Series X",
                price: 4299m,
                category: "Consoles",
                description: "O console Xbox mais poderoso já feito com 12 TFLOPS de GPU, 1 TB de SSD e retrocompatibilidade com 4 gerações de jogos.",
                imageUrl: "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
                stock: 18,
                discountPercentage: 0,
                brand: "Microsoft"),
            ("CPU", "AMD Zen 2 — 3,8 GHz (8 núcleos)"),
            ("GPU", "AMD RDNA 2 — 12 TFLOPS"),
            ("RAM", "16 GB GDDR6"),
            ("Armazenamento", "1 TB SSD NVMe"),
            ("Resolução", "Até 8K"),
            ("Frame Rate", "Até 120 fps")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Nintendo Switch OLED",
                price: 2699m,
                category: "Consoles",
                description: "A versão premium do Switch com tela OLED de 7\", suporte ajustável aprimorado e 64 GB de armazenamento interno.",
                imageUrl: "https://images.unsplash.com/photo-1585987561578-e46f5e3db2a1?w=600&auto=format&fit=crop&q=80",
                stock: 35,
                discountPercentage: 0,
                brand: "Nintendo"),
            ("Tela", "7\" OLED 1.280 x 720"),
            ("Modos", "TV, Mesa e Portátil"),
            ("Armazenamento", "64 GB (expansível microSD)"),
            ("Bateria", "4 – 9 horas"),
            ("Resolução na TV", "1080p"),
            ("Áudio", "Alto-falante aprimorado")));

        list.Add(WithSpecs(
            Product.Create(
                name: "PlayStation 5 Digital Edition",
                price: 3599m,
                category: "Consoles",
                description: "Versão sem leitor de disco do PS5, com todo o poder do console original em design ainda mais fino.",
                imageUrl: "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=600&auto=format&fit=crop&q=80",
                stock: 12,
                discountPercentage: 0,
                brand: "Sony"),
            ("CPU", "AMD Zen 2 — 3,5 GHz (8 núcleos)"),
            ("GPU", "AMD RDNA 2 — 10,3 TFLOPS"),
            ("RAM", "16 GB GDDR6"),
            ("Armazenamento", "1 TB SSD NVMe"),
            ("Resolução", "Até 4K / 120fps"),
            ("Leitor", "Digital (sem disco)")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Xbox Series S",
                price: 2499m,
                category: "Consoles",
                description: "O Xbox mais compacto e acessível, com 512 GB de SSD, até 120fps e acesso ao Xbox Game Pass.",
                imageUrl: "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600&auto=format&fit=crop&q=80",
                stock: 30,
                discountPercentage: 0,
                brand: "Microsoft"),
            ("CPU", "AMD Zen 2 — 3,6 GHz (8 núcleos)"),
            ("GPU", "AMD RDNA 2 — 4 TFLOPS"),
            ("RAM", "10 GB GDDR6"),
            ("Armazenamento", "512 GB SSD NVMe"),
            ("Resolução", "Até 1440p / 120fps"),
            ("Game Pass", "Compatível")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Steam Deck OLED",
                price: 4499m,
                category: "Consoles",
                description: "Versão OLED do Steam Deck com tela de 7,4\" HDR, bateria 50% maior e processador AMD melhorado.",
                imageUrl: "https://images.unsplash.com/photo-1592899677977-9c10002761e8?w=600&auto=format&fit=crop&q=80",
                stock: 15,
                discountPercentage: 0,
                brand: "Valve"),
            ("CPU", "AMD Zen 2 (8 núcleos / 16 threads)"),
            ("GPU", "AMD RDNA 2 — 1,6 TFLOPS"),
            ("RAM", "16 GB LPDDR5"),
            ("Armazenamento", "512 GB NVMe SSD"),
            ("Tela", "7,4\" OLED HDR 90Hz"),
            ("Bateria", "50 Wh — até 12 horas")));

        // ── Componentes ──────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "Placa de Vídeo ASUS ROG RTX 4070 Ti Super",
                price: 5499m,
                category: "Componentes",
                description: "RTX 4070 Ti Super com 16 GB GDDR6X, triple-fan ROG Strix e overclocking de fábrica para jogos em 4K e criação de conteúdo.",
                imageUrl: "https://images.unsplash.com/photo-1591405351990-4726e331f141?w=600&auto=format&fit=crop&q=80",
                stock: 10,
                discountPercentage: 0,
                brand: "Asus"),
            ("VRAM", "16 GB GDDR6X"),
            ("Barramento", "256-bit"),
            ("CUDA Cores", "8.448"),
            ("Boost Clock", "2.670 MHz"),
            ("TDP", "285W"),
            ("Conectores", "3x DisplayPort 1.4a + 1x HDMI 2.1")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Processador Intel Core i9-14900K",
                price: 3299m,
                category: "Componentes",
                description: "CPU de alta performance com 24 núcleos (8P+16E), até 6,0 GHz de boost e suporte a DDR5 para workstations e gaming extremo.",
                imageUrl: "https://images.unsplash.com/photo-1591267990532-e5bdb1b0cae8?w=600&auto=format&fit=crop&q=80",
                stock: 14,
                discountPercentage: 5,
                brand: "Intel"),
            ("Núcleos / Threads", "24 núcleos / 32 threads"),
            ("Frequência Base P-Core", "3,2 GHz"),
            ("Frequência Boost", "6,0 GHz"),
            ("Cache", "36 MB L3"),
            ("Socket", "LGA 1700"),
            ("TDP", "125W (253W PL2)")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Memória RAM Corsair Vengeance 32 GB DDR5",
                price: 899m,
                category: "Componentes",
                description: "Kit de 2x16 GB DDR5-6000 com XMP 3.0 e perfil baixo para compatibilidade com coolers grandes.",
                imageUrl: "https://images.unsplash.com/photo-1562976540-1502c2145851?w=600&auto=format&fit=crop&q=80",
                stock: 45,
                discountPercentage: 0,
                brand: "Corsair"),
            ("Capacidade", "32 GB (2x16 GB)"),
            ("Tipo", "DDR5"),
            ("Frequência", "6.000 MHz"),
            ("Latência", "CL30"),
            ("Voltagem", "1,35V"),
            ("Perfil", "XMP 3.0 / EXPO")));

        list.Add(WithSpecs(
            Product.Create(
                name: "SSD Samsung 990 Pro 2 TB",
                price: 1199m,
                category: "Componentes",
                description: "NVMe PCIe 4.0 com leituras de até 7.450 MB/s, ideal para jogos, edição de vídeo e criação de conteúdo profissional.",
                imageUrl: "https://images.unsplash.com/photo-1597852074816-d933c7d2b988?w=600&auto=format&fit=crop&q=80",
                stock: 38,
                discountPercentage: 10,
                brand: "Samsung"),
            ("Interface", "PCIe 4.0 NVMe M.2"),
            ("Capacidade", "2 TB"),
            ("Leitura Sequencial", "7.450 MB/s"),
            ("Escrita Sequencial", "6.900 MB/s"),
            ("TBW", "1.200 TBW"),
            ("Garantia", "5 anos")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Placa-mãe ASUS ROG Strix Z790-E Gaming",
                price: 2799m,
                category: "Componentes",
                description: "Placa-mãe topo de linha para Intel Core de 12ª/13ª/14ª geração com Wi-Fi 6E, DDR5, PCIe 5.0 e VRM de 18+1 fases.",
                imageUrl: "https://images.unsplash.com/photo-1518770660439-4636190af475?w=600&auto=format&fit=crop&q=80",
                stock: 12,
                discountPercentage: 0,
                brand: "Asus"),
            ("Socket", "LGA 1700"),
            ("Chipset", "Intel Z790"),
            ("Memória", "DDR5 — 4 slots / até 192 GB"),
            ("Slots PCIe", "PCIe 5.0 x16 + PCIe 4.0 x16"),
            ("Wi-Fi", "Wi-Fi 6E + Bluetooth 5.3"),
            ("VRM", "18+1 fases")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Cooler CPU Noctua NH-D15 G2",
                price: 899m,
                category: "Componentes",
                description: "O melhor cooler air-cooling do mercado, com dois fans NF-A15 PWM e suporte a todos os sockets modernos Intel e AMD.",
                imageUrl: "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=600&auto=format&fit=crop&q=80",
                stock: 28,
                discountPercentage: 0,
                brand: "Noctua"),
            ("Tipo", "Dupla torre com 2 fans"),
            ("Fans", "2x NF-A15 PWM 140mm"),
            ("RPM", "300 – 1.500 RPM"),
            ("Ruído", "Até 24,6 dB(A)"),
            ("Compatibilidade", "LGA 1700, AM5, AM4 e mais"),
            ("Altura", "165 mm")));

        list.Add(WithSpecs(
            Product.Create(
                name: "AMD Ryzen 9 7950X",
                price: 3799m,
                category: "Componentes",
                description: "Processador AMD de 16 núcleos e 32 threads com até 5,7 GHz de boost, ideal para criação de conteúdo, streaming e workstations.",
                imageUrl: "https://images.unsplash.com/photo-1591267990532-e5bdb1b0cae8?w=600&auto=format&fit=crop&q=80",
                stock: 10,
                discountPercentage: 0,
                brand: "AMD"),
            ("Núcleos / Threads", "16 núcleos / 32 threads"),
            ("Frequência Base", "4,5 GHz"),
            ("Frequência Boost", "5,7 GHz"),
            ("Cache", "64 MB L3 + 16 MB L2"),
            ("Socket", "AM5"),
            ("TDP", "170W")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Placa de Vídeo XFX RX 7900 XTX",
                price: 5999m,
                category: "Componentes",
                description: "RX 7900 XTX com 24 GB GDDR6 e arquitetura RDNA 3 para gaming 4K nativo com ray tracing e desempenho profissional.",
                imageUrl: "https://images.unsplash.com/photo-1591405351990-4726e331f141?w=600&auto=format&fit=crop&q=80",
                stock: 8,
                discountPercentage: 0,
                brand: "AMD"),
            ("VRAM", "24 GB GDDR6"),
            ("Barramento", "384-bit"),
            ("Compute Units", "96 CU"),
            ("Boost Clock", "2.499 MHz"),
            ("TDP", "355W"),
            ("Conectores", "2x DisplayPort 2.1 + 1x HDMI 2.1 + 1x USB-C")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Fonte Corsair RM1000x 1000W",
                price: 1299m,
                category: "Componentes",
                description: "Fonte modular 80 Plus Gold de 1000W com modo zero RPM, cabos flat e 10 anos de garantia.",
                imageUrl: "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=600&auto=format&fit=crop&q=80",
                stock: 20,
                discountPercentage: 0,
                brand: "Corsair"),
            ("Potência", "1.000W"),
            ("Certificação", "80 Plus Gold"),
            ("Modularidade", "Totalmente modular"),
            ("Modo Zero RPM", "Silencioso em baixa carga"),
            ("Fan", "135mm FDB"),
            ("Garantia", "10 anos")));

        list.Add(WithSpecs(
            Product.Create(
                name: "Gabinete Lian Li PC-O11 Dynamic EVO",
                price: 1499m,
                category: "Componentes",
                description: "Gabinete Mid-Tower dual-chamber com suporte a 3x360mm de água, vidro temperado e layout invertido opcional.",
                imageUrl: "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=600&auto=format&fit=crop&q=80",
                stock: 18,
                discountPercentage: 0,
                brand: "Lian Li"),
            ("Fator de Forma", "Mid-Tower"),
            ("Painel", "Vidro temperado (3 lados)"),
            ("Radiadores", "Até 3x 360mm"),
            ("Baias 3.5\"", "2 (expansível)"),
            ("Baias 2.5\"", "4"),
            ("Dimensões", "285 x 459 x 465 mm")));

        return list;
    }

    private static Product WithSpecs(Product product, params (string Label, string Value)[] specs)
    {
        var specEntities = specs
            .Select((s, i) => ProductSpecification.Create(product.Id, s.Label, s.Value, i))
            .ToList();

        product.SetSpecifications(specEntities);
        return product;
    }
}
