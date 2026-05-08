using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProductService.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseSqlite("Data Source=mcart-products.db");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    // db.Database.EnsureCreated(); // Removed for production safety

    // Seed categories and products per SRS §9 Product Catalogue
    if (!db.Categories.Any())
    {
        // ── Root categories ──────────────────────────────────────────────────
        var womenId   = Guid.NewGuid();
        var menId     = Guid.NewGuid();

        // ── Women sub-categories ─────────────────────────────────────────────
        var wIndianId    = Guid.NewGuid(); // Indian & Western Wear
        var wWesternId   = Guid.NewGuid(); // Western Wear
        var wAccessId    = Guid.NewGuid(); // Accessories

        // Women → Indian & Western Wear
        var wKurtasId    = Guid.NewGuid();
        var wKurtisId    = Guid.NewGuid();
        var wLeggingsId  = Guid.NewGuid();
        var wSkirtsId    = Guid.NewGuid();
        var wSareeId     = Guid.NewGuid();
        var wDressMatId  = Guid.NewGuid();
        var wLehengaId   = Guid.NewGuid();
        var wDupattaId   = Guid.NewGuid();

        // Women → Western Wear
        var wDressesId   = Guid.NewGuid();
        var wTopsId      = Guid.NewGuid();
        var wJeansId     = Guid.NewGuid();
        var wTrousersId  = Guid.NewGuid();
        var wShortsId    = Guid.NewGuid();
        var wShrugsId    = Guid.NewGuid();
        var wSweatersId  = Guid.NewGuid();
        var wJacketsId   = Guid.NewGuid();
        var wCoatsId     = Guid.NewGuid();

        // Women → Accessories
        var wWatchesId   = Guid.NewGuid();
        var wSunglassId  = Guid.NewGuid();
        var wEyeglassId  = Guid.NewGuid();
        var wBeltId      = Guid.NewGuid();

        // ── Men sub-categories ────────────────────────────────────────────────
        var mClothingId  = Guid.NewGuid();
        var mAccessId    = Guid.NewGuid();

        // Men → Clothing
        var mTshirtsId   = Guid.NewGuid();
        var mCasualShirtId  = Guid.NewGuid();
        var mFormalShirtId  = Guid.NewGuid();
        var mSuitsId     = Guid.NewGuid();
        var mJeansId     = Guid.NewGuid();
        var mCasualTrId  = Guid.NewGuid();
        var mFormalTrId  = Guid.NewGuid();
        var mShortsId    = Guid.NewGuid();
        var mTrackId     = Guid.NewGuid();
        var mSweatersId  = Guid.NewGuid();
        var mJacketsId   = Guid.NewGuid();
        var mBlazersId   = Guid.NewGuid();
        var mSportsId    = Guid.NewGuid();
        var mIndianId    = Guid.NewGuid();
        var mInnerwearId = Guid.NewGuid();

        // Men → Accessories
        var mWatchesId   = Guid.NewGuid();
        var mSunglassId  = Guid.NewGuid();
        var mBagsId      = Guid.NewGuid();
        var mLuggageId   = Guid.NewGuid();
        var mGroomingId  = Guid.NewGuid();
        var mWalletsId   = Guid.NewGuid();
        var mFashionAccId = Guid.NewGuid();

        db.Categories.AddRange(
            // Root
            new ProductService.Models.Category { CategoryId = womenId,  Name = "Women",  Gender = "Women" },
            new ProductService.Models.Category { CategoryId = menId,    Name = "Men",    Gender = "Men"   },

            // Women level-1
            new ProductService.Models.Category { CategoryId = wIndianId,  Name = "Indian & Western Wear", Gender = "Women", ParentId = womenId },
            new ProductService.Models.Category { CategoryId = wWesternId, Name = "Western Wear",          Gender = "Women", ParentId = womenId },
            new ProductService.Models.Category { CategoryId = wAccessId,  Name = "Accessories",           Gender = "Women", ParentId = womenId },

            // Women → Indian & Western Wear
            new ProductService.Models.Category { CategoryId = wKurtasId,   Name = "Kurtas & Suits",                  Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wKurtisId,   Name = "Kurtis & Tunics",                 Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wLeggingsId, Name = "Leggings, Salwars & Churidars",   Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wSkirtsId,   Name = "Skirts & Palazzos",               Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wSareeId,    Name = "Sarees & Blouses",                Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wDressMatId, Name = "Dress Material",                  Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wLehengaId,  Name = "Lehenga Choli",                   Gender = "Women", ParentId = wIndianId },
            new ProductService.Models.Category { CategoryId = wDupattaId,  Name = "Dupattas & Shawls",               Gender = "Women", ParentId = wIndianId },

            // Women → Western Wear
            new ProductService.Models.Category { CategoryId = wDressesId,  Name = "Dresses & Jumpsuits",   Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wTopsId,     Name = "Tops, T-Shirts & Shirts",Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wJeansId,    Name = "Jeans & Jeggings",       Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wTrousersId, Name = "Trousers & Capris",      Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wShortsId,   Name = "Shorts & Skirts",        Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wShrugsId,   Name = "Shrugs",                 Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wSweatersId, Name = "Sweaters & Sweatshirts", Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wJacketsId,  Name = "Jackets & Waistcoats",   Gender = "Women", ParentId = wWesternId },
            new ProductService.Models.Category { CategoryId = wCoatsId,    Name = "Coats & Blazers",        Gender = "Women", ParentId = wWesternId },

            // Women → Accessories
            new ProductService.Models.Category { CategoryId = wWatchesId,  Name = "Women Watches",  Gender = "Women", ParentId = wAccessId },
            new ProductService.Models.Category { CategoryId = wSunglassId, Name = "Sunglasses",      Gender = "Women", ParentId = wAccessId },
            new ProductService.Models.Category { CategoryId = wEyeglassId, Name = "Eye Glasses",     Gender = "Women", ParentId = wAccessId },
            new ProductService.Models.Category { CategoryId = wBeltId,     Name = "Belt",            Gender = "Women", ParentId = wAccessId },

            // Men level-1
            new ProductService.Models.Category { CategoryId = mClothingId, Name = "Clothing",    Gender = "Men", ParentId = menId },
            new ProductService.Models.Category { CategoryId = mAccessId,   Name = "Accessories", Gender = "Men", ParentId = menId },

            // Men → Clothing
            new ProductService.Models.Category { CategoryId = mTshirtsId,    Name = "T-Shirts",                Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mCasualShirtId,Name = "Casual Shirts",           Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mFormalShirtId,Name = "Formal Shirts",           Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mSuitsId,      Name = "Suits",                   Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mJeansId,      Name = "Jeans",                   Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mCasualTrId,   Name = "Casual Trousers",         Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mFormalTrId,   Name = "Formal Trousers",         Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mShortsId,     Name = "Shorts",                  Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mTrackId,      Name = "Track Pants",             Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mSweatersId,   Name = "Sweaters & Sweatshirts",  Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mJacketsId,    Name = "Jackets",                 Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mBlazersId,    Name = "Blazers & Coats",         Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mSportsId,     Name = "Sports & Active Wear",    Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mIndianId,     Name = "Indian & Festive Wear",   Gender = "Men", ParentId = mClothingId },
            new ProductService.Models.Category { CategoryId = mInnerwearId,  Name = "Innerwear & Sleepwear",   Gender = "Men", ParentId = mClothingId },

            // Men → Accessories
            new ProductService.Models.Category { CategoryId = mWatchesId,    Name = "Watches & Wearables",        Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mSunglassId,   Name = "Sunglasses & Frames",         Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mBagsId,       Name = "Bags & Backpacks",            Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mLuggageId,    Name = "Luggage & Trolleys",          Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mGroomingId,   Name = "Personal Care & Grooming",    Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mWalletsId,    Name = "Wallets & Belts",             Gender = "Men", ParentId = mAccessId },
            new ProductService.Models.Category { CategoryId = mFashionAccId, Name = "Fashion Accessories",         Gender = "Men", ParentId = mAccessId }
        );

        db.Products.AddRange(
            // ── Women → Indian & Western Wear ─────────────────────────────────
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Embroidered Cotton Kurta Set",       Description = "Elegant white embroidered cotton kurta with matching salwar suits. Perfect for office or casual wear.",         CategoryId = wKurtasId,   Price = 59.99m,  Discount = 10, Stock = 80,  Tags = "women,kurta,cotton,white,ethnic",        IsFeatured = true,  IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Printed Rayon Kurti",                Description = "Vibrant floral printed rayon kurti, knee-length, available in multiple colours.",                              CategoryId = wKurtisId,   Price = 29.99m,  Discount = 0,  Stock = 120, Tags = "women,kurti,rayon,printed,floral",       IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Churidar Leggings Combo Pack",       Description = "Comfortable cotton churidar leggings — pack of 3 in classic solid colours.",                                  CategoryId = wLeggingsId, Price = 19.99m,  Discount = 5,  Stock = 200, Tags = "women,leggings,churidar,comfort,pack",   IsFeatured = false, IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Flared Palazzo Pants",               Description = "Trending wide-leg palazzo pants in georgette fabric with ethnic block print.",                                 CategoryId = wSkirtsId,   Price = 34.99m,  Discount = 0,  Stock = 90,  Tags = "women,palazzo,georgette,ethnic,block",   IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Banarasi Silk Saree",                Description = "Luxurious Banarasi silk saree in deep red with golden zari border. Includes matching blouse piece.",            CategoryId = wSareeId,    Price = 199.99m, Discount = 15, Stock = 40,  Tags = "women,saree,silk,banarasi,wedding",      IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Floral Cotton Dress Material",       Description = "Unstitched floral cotton dress material with dupatta — ideal for custom tailoring.",                           CategoryId = wDressMatId, Price = 24.99m,  Discount = 0,  Stock = 150, Tags = "women,dressmaterial,cotton,floral",      IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Bridal Lehenga Choli",               Description = "Stunning bridal lehenga choli in maroon velvet with heavy embroidery and dupatta. Perfect for weddings.",      CategoryId = wLehengaId,  Price = 499.99m, Discount = 20, Stock = 15,  Tags = "women,lehenga,bridal,velvet,wedding",    IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Chiffon Dupatta",                    Description = "Lightweight chiffon dupatta with lace border in pastel shades. Pairs with any Indian outfit.",                  CategoryId = wDupattaId,  Price = 14.99m,  Discount = 0,  Stock = 180, Tags = "women,dupatta,chiffon,lace,pastel",      IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },

            // ── Women → Western Wear ───────────────────────────────────────────
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Floral Sundress",                    Description = "Breezy floral wrap sundress with adjustable tie waist. Light and casual for summer.",                          CategoryId = wDressesId,  Price = 54.99m,  Discount = 10, Stock = 75,  Tags = "women,dress,floral,summer,casual",       IsFeatured = true,  IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Striped Cotton Jumpsuit",            Description = "Chic black-and-white striped cotton jumpsuit with pockets and adjustable straps.",                            CategoryId = wDressesId,  Price = 69.99m,  Discount = 0,  Stock = 55,  Tags = "women,jumpsuit,striped,cotton,chic",     IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Fitted Graphic Tee",                 Description = "Soft jersey graphic tee with motivational print. Available in XS-XL.",                                       CategoryId = wTopsId,     Price = 19.99m,  Discount = 0,  Stock = 160, Tags = "women,tshirt,graphic,jersey,casual",     IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Lace Detail Blouse",                 Description = "Elegant lace-trim blouse in ivory — perfect for formal or evening occasions.",                                CategoryId = wTopsId,     Price = 39.99m,  Discount = 5,  Stock = 70,  Tags = "women,blouse,lace,ivory,formal",         IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "High-Waist Skinny Jeans",            Description = "Classic high-waist skinny jeans in dark indigo wash. Stretch denim for comfort.",                             CategoryId = wJeansId,    Price = 59.99m,  Discount = 15, Stock = 100, Tags = "women,jeans,skinny,highwaist,denim",     IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Ankle Jeggings",                     Description = "Super-stretch ankle jeggings that look like jeans but feel like leggings.",                                    CategoryId = wJeansId,    Price = 44.99m,  Discount = 0,  Stock = 85,  Tags = "women,jeggings,ankle,stretch,comfort",   IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Tailored Capri Trousers",            Description = "Slim-fit capri trousers in beige cotton blend — smart casual for all occasions.",                             CategoryId = wTrousersId, Price = 49.99m,  Discount = 0,  Stock = 65,  Tags = "women,capri,trousers,beige,smartcasual", IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Denim Shorts",                       Description = "Distressed denim shorts with frayed hem. Great for casual summer outings.",                                   CategoryId = wShortsId,   Price = 34.99m,  Discount = 10, Stock = 110, Tags = "women,shorts,denim,distressed,summer",   IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Knit Open-Front Shrug",              Description = "Cozy knit open-front shrug — the perfect layering piece over dresses and tops.",                              CategoryId = wShrugsId,   Price = 29.99m,  Discount = 0,  Stock = 90,  Tags = "women,shrug,knit,layering,cozy",         IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Hooded Fleece Sweatshirt",           Description = "Warm fleece-lined pullover hoodie in dusty pink — soft and cozy for cooler days.",                           CategoryId = wSweatersId, Price = 49.99m,  Discount = 0,  Stock = 95,  Tags = "women,hoodie,fleece,sweatshirt,pink",    IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Quilted Puffer Jacket",              Description = "Lightweight quilted puffer jacket with stand collar. Keeps you warm without the bulk.",                        CategoryId = wJacketsId,  Price = 99.99m,  Discount = 20, Stock = 40,  Tags = "women,jacket,puffer,quilted,warmth",     IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Double-Breasted Blazer",             Description = "Tailored double-breasted blazer in charcoal grey. Sharp and professional.",                                   CategoryId = wCoatsId,    Price = 119.99m, Discount = 0,  Stock = 30,  Tags = "women,blazer,formal,charcoal,tailored",  IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },

            // ── Women → Accessories ────────────────────────────────────────────
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Rose Gold Analog Watch",             Description = "Elegant rose gold analog watch with leather strap and a minimalist dial.",                                    CategoryId = wWatchesId,  Price = 89.99m,  Discount = 10, Stock = 50,  Tags = "women,watch,rosegold,analog,leather",    IsFeatured = true,  IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Cat-Eye Sunglasses",                 Description = "Retro cat-eye sunglasses with UV400 protection. Trendy and protective.",                                     CategoryId = wSunglassId, Price = 24.99m,  Discount = 0,  Stock = 130, Tags = "women,sunglasses,cateye,retro,uv400",    IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Blue Light Blocking Glasses",        Description = "Stylish blue-light blocking glasses with anti-glare coating — ideal for screen time.",                        CategoryId = wEyeglassId, Price = 34.99m,  Discount = 5,  Stock = 80,  Tags = "women,glasses,bluelight,antiglare",      IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Woven Leather Belt",                 Description = "Chic woven leather belt with a gold pin buckle. Fits waist sizes 24-36.",                                    CategoryId = wBeltId,     Price = 19.99m,  Discount = 0,  Stock = 100, Tags = "women,belt,leather,woven,gold",          IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },

            // ── Men → Clothing ─────────────────────────────────────────────────
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Classic White Crew-Neck T-Shirt",    Description = "100% cotton crew-neck tee in crisp white. A timeless wardrobe essential.",                                   CategoryId = mTshirtsId,    Price = 24.99m,  Discount = 0,  Stock = 200, Tags = "men,tshirt,white,crewneck,cotton",        IsFeatured = true,  IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Graphic Printed Round-Neck Tee",     Description = "Bold street-art graphic print on premium soft jersey. Available in black and navy.",                         CategoryId = mTshirtsId,    Price = 29.99m,  Discount = 10, Stock = 150, Tags = "men,tshirt,graphic,streetwear,jersey",    IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Linen Casual Shirt",                 Description = "Relaxed-fit linen casual shirt in sky blue. Light and breathable for summer.",                               CategoryId = mCasualShirtId, Price = 44.99m,  Discount = 0,  Stock = 90,  Tags = "men,shirt,linen,casual,blue,summer",      IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Plaid Flannel Shirt",                Description = "Classic red-and-black plaid flannel shirt — perfect for layering in autumn.",                               CategoryId = mCasualShirtId, Price = 49.99m,  Discount = 5,  Stock = 75,  Tags = "men,shirt,flannel,plaid,autumn,casual",   IsFeatured = false, IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Slim-Fit Formal Shirt",              Description = "Non-iron slim-fit formal shirt in solid white. Essential for any business wardrobe.",                        CategoryId = mFormalShirtId, Price = 54.99m,  Discount = 10, Stock = 100, Tags = "men,shirt,formal,slimfit,white,office",   IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Striped Oxford Formal Shirt",        Description = "Fine-stripe Oxford weave formal shirt in light blue — smart and professional.",                             CategoryId = mFormalShirtId, Price = 59.99m,  Discount = 0,  Stock = 70,  Tags = "men,shirt,formal,oxford,striped,blue",    IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "3-Piece Business Suit",              Description = "Premium wool-blend 3-piece business suit in charcoal. Includes jacket, vest, and trousers.",                CategoryId = mSuitsId,      Price = 299.99m, Discount = 15, Stock = 25,  Tags = "men,suit,business,wool,charcoal,formal",  IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Slim-Fit Dark Wash Jeans",           Description = "Classic slim-fit jeans in dark indigo wash with slight stretch for comfort.",                               CategoryId = mJeansId,      Price = 64.99m,  Discount = 0,  Stock = 110, Tags = "men,jeans,slimfit,darkwash,denim",        IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Distressed Jogger Jeans",            Description = "Relaxed distressed jeans with elasticated ankles — street style meets comfort.",                            CategoryId = mJeansId,      Price = 54.99m,  Discount = 10, Stock = 85,  Tags = "men,jeans,distressed,jogger,streetstyle", IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Chino Casual Trousers",              Description = "Slim-fit cotton chino trousers in khaki. Great for smart-casual occasions.",                                 CategoryId = mCasualTrId,   Price = 49.99m,  Discount = 0,  Stock = 95,  Tags = "men,chino,casual,khaki,smartcasual",      IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Slim-Fit Formal Trousers",           Description = "Wrinkle-resistant slim-fit formal trousers in black. Office-ready and polished.",                           CategoryId = mFormalTrId,   Price = 59.99m,  Discount = 5,  Stock = 80,  Tags = "men,trousers,formal,slimfit,black,office",IsFeatured = false, IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Cargo Shorts",                       Description = "Multi-pocket cargo shorts in olive green — rugged and practical for outdoor activities.",                   CategoryId = mShortsId,     Price = 34.99m,  Discount = 0,  Stock = 120, Tags = "men,shorts,cargo,olive,outdoor",          IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Dry-Fit Track Pants",                Description = "Lightweight dry-fit track pants with side zip pockets and elasticated waist. Ideal for workouts.",          CategoryId = mTrackId,      Price = 39.99m,  Discount = 0,  Stock = 140, Tags = "men,trackpants,dryfit,workout,gym",       IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Cable-Knit Sweater",                 Description = "Classic cable-knit woollen sweater in oatmeal beige — warm and timeless.",                                  CategoryId = mSweatersId,   Price = 79.99m,  Discount = 10, Stock = 60,  Tags = "men,sweater,cableknit,wool,beige,winter", IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Zip-Up Bomber Jacket",               Description = "Sleek satin bomber jacket with ribbed cuffs and collar. A street-style staple.",                           CategoryId = mJacketsId,    Price = 89.99m,  Discount = 0,  Stock = 50,  Tags = "men,jacket,bomber,satin,streetstyle",     IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Single-Breasted Blazer",             Description = "Classic single-breasted blazer in navy blue. Versatile for both smart-casual and formal settings.",         CategoryId = mBlazersId,    Price = 139.99m, Discount = 15, Stock = 35,  Tags = "men,blazer,navy,formal,smartcasual",      IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Sports Shorts & T-Shirt Set",        Description = "Matching moisture-wicking sports shorts and T-shirt set. Performance fabric for gym or running.",           CategoryId = mSportsId,     Price = 54.99m,  Discount = 10, Stock = 100, Tags = "men,sports,set,gym,running,activewear",   IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Embroidered Kurta Pajama Set",       Description = "Traditional embroidered cotton kurta pajama set in ivory — ideal for festivals and puja.",                   CategoryId = mIndianId,     Price = 74.99m,  Discount = 0,  Stock = 60,  Tags = "men,kurta,pajama,ethnic,festival,ivory",  IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Premium Cotton Boxer Briefs (Pack 3)",Description = "Breathable premium cotton boxer briefs in classic colours — pack of 3 for everyday comfort.",             CategoryId = mInnerwearId,  Price = 29.99m,  Discount = 0,  Stock = 300, Tags = "men,innerwear,boxers,cotton,comfort,pack",IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },

            // ── Men → Accessories ──────────────────────────────────────────────
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Chronograph Sports Watch",           Description = "Stainless steel chronograph watch with tachymeter bezel and silicone strap. Water resistant 50m.",          CategoryId = mWatchesId,    Price = 149.99m, Discount = 20, Stock = 45,  Tags = "men,watch,chronograph,sports,steel",      IsFeatured = true,  IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Polarized Aviator Sunglasses",       Description = "Classic polarized aviator sunglasses with UV400 lenses and gold metal frame.",                             CategoryId = mSunglassId,   Price = 39.99m,  Discount = 0,  Stock = 110, Tags = "men,sunglasses,aviator,polarized,uv400",  IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Canvas Laptop Backpack",             Description = "Durable 30L canvas backpack with dedicated laptop compartment and USB charging port.",                      CategoryId = mBagsId,       Price = 69.99m,  Discount = 10, Stock = 70,  Tags = "men,backpack,canvas,laptop,travel",       IsFeatured = true,  IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Hard-Shell Trolley Suitcase 24\"",   Description = "Lightweight ABS hard-shell trolley suitcase with 360° spinner wheels and TSA lock.",                       CategoryId = mLuggageId,    Price = 119.99m, Discount = 15, Stock = 30,  Tags = "men,luggage,trolley,hardsell,travel",     IsFeatured = false, IsNew = false, IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "6-in-1 Grooming Kit",               Description = "Complete grooming kit with electric trimmer, beard comb, scissors, and accessories. Cordless 60-min run.",  CategoryId = mGroomingId,   Price = 49.99m,  Discount = 0,  Stock = 85,  Tags = "men,grooming,trimmer,beard,kit",          IsFeatured = true,  IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Slim RFID Leather Wallet",           Description = "Minimalist slim RFID-blocking genuine leather wallet — holds up to 8 cards.",                             CategoryId = mWalletsId,    Price = 34.99m,  Discount = 5,  Stock = 130, Tags = "men,wallet,rfid,leather,slim,cards",      IsFeatured = false, IsNew = true,  IsOnSale = true,  CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Braided Canvas Belt",                Description = "Casual braided canvas belt with automatic buckle. Fits waist 28-40.",                                      CategoryId = mWalletsId,    Price = 19.99m,  Discount = 0,  Stock = 160, Tags = "men,belt,canvas,braided,casual",          IsFeatured = false, IsNew = false, IsOnSale = false, CreatedAt = DateTime.UtcNow },
            new ProductService.Models.Product { ProductId = Guid.NewGuid(), Name = "Beanie Knit Cap",                    Description = "Warm ribbed-knit beanie in charcoal grey with fold-up cuff. One size fits most.",                          CategoryId = mFashionAccId, Price = 14.99m,  Discount = 0,  Stock = 200, Tags = "men,beanie,knit,cap,winter,charcoal",     IsFeatured = false, IsNew = true,  IsOnSale = false, CreatedAt = DateTime.UtcNow }
        );

        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
