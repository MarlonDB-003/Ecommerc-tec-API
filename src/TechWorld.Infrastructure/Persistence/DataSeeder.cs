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
        if (await context.Products.AnyAsync())
            return;

        var products = BuildProducts();
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
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
                imageUrl: "https://images.samsung.com/is/image/samsung/p6pim/br/sm-s928b/gallery/br-galaxy-s24-ultra-s928-sm-s928bzkpzto-thumb-539207961",
                stock: 40,
                discountPercentage: 15),
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
                description: "O iPhone 16 Pro Max apresenta o chip A18 Pro, câmera Fusion de 48 MP e a maior tela Pro já feita com titanio de grau aeroespacial.",
                imageUrl: "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-16-pro-finish-select-202409-6-9inch-naturaltitanium",
                stock: 25,
                discountPercentage: 10),
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
                imageUrl: null,
                stock: 60,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 80,
                discountPercentage: 20),
            ("Processador", "Snapdragon 7s Gen 3"),
            ("RAM", "8 GB"),
            ("Armazenamento", "256 GB"),
            ("Câmera Principal", "200 MP"),
            ("Bateria", "5.500 mAh"),
            ("Tela", "6,67\" AMOLED 120Hz")));

        // ── Computadores ─────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "MacBook Air 15\" M3",
                price: 12999m,
                category: "Computadores",
                description: "Incrivelmente fino e leve com o chip M3 da Apple. Até 18 horas de bateria, tela Liquid Retina de 15,3\" e câmera FaceTime 1080p.",
                imageUrl: null,
                stock: 18,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 35,
                discountPercentage: 10),
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
                imageUrl: null,
                stock: 8,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 50,
                discountPercentage: 5),
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
                imageUrl: null,
                stock: 70,
                discountPercentage: 15),
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
                imageUrl: null,
                stock: 55,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 30,
                discountPercentage: 10),
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
                imageUrl: null,
                stock: 22,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 15,
                discountPercentage: 0),
            ("Material", "Couro genuíno italiano"),
            ("Inclinação", "Até 165°"),
            ("Altura do assento", "40 – 51 cm"),
            ("Largura do assento", "56 cm"),
            ("Capacidade", "até 120 kg"),
            ("Base", "Alumínio polido")));

        // ── Consoles ─────────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "PlayStation 5 Slim",
                price: 3999m,
                category: "Consoles",
                description: "O PS5 Slim chega 30% menor que o original com leitor de disco removível, 1 TB de SSD e suporte a jogos em 4K/120fps.",
                imageUrl: null,
                stock: 20,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 18,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 35,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 12,
                discountPercentage: 0),
            ("CPU", "AMD Zen 2 — 3,5 GHz (8 núcleos)"),
            ("GPU", "AMD RDNA 2 — 10,3 TFLOPS"),
            ("RAM", "16 GB GDDR6"),
            ("Armazenamento", "1 TB SSD NVMe"),
            ("Resolução", "Até 4K / 120fps"),
            ("Leitor", "Digital (sem disco)")));

        // ── Componentes ──────────────────────────────────────────────
        list.Add(WithSpecs(
            Product.Create(
                name: "Placa de Vídeo ASUS ROG RTX 4070 Ti Super",
                price: 5499m,
                category: "Componentes",
                description: "RTX 4070 Ti Super com 16 GB GDDR6X, triple-fan ROG Strix e overclocking de fábrica para jogos em 4K e criação de conteúdo.",
                imageUrl: null,
                stock: 10,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 14,
                discountPercentage: 5),
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
                imageUrl: null,
                stock: 45,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 38,
                discountPercentage: 10),
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
                imageUrl: null,
                stock: 12,
                discountPercentage: 0),
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
                imageUrl: null,
                stock: 28,
                discountPercentage: 0),
            ("Tipo", "Dupla torre com 2 fans"),
            ("Fans", "2x NF-A15 PWM 140mm"),
            ("RPM", "300 – 1.500 RPM"),
            ("Ruído", "Até 24,6 dB(A)"),
            ("Compatibilidade", "LGA 1700, AM5, AM4 e mais"),
            ("Altura", "165 mm")));

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
