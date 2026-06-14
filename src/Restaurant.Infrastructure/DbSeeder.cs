using Microsoft.EntityFrameworkCore;
using Restaurant.Domain;

namespace Restaurant.Infrastructure;

/// <summary>Creates the database and seeds the live Viva Italia menu on first run.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(MenuDbContext db)
    {
        // Schema is ensured by DbInitializer.InitAsync; the admin account by EnsureAdminAsync.
        if (await db.Categories.AnyAsync()) return;

        // ---- Categories ----
        var chicken = Cat("chicken-crispy", "🍗", 1,
            ("Chicken & Crispy", "Wings, crispy chicken & loaded fries"),
            ("دجاج وكريسبي", "أجنحة، دجاج مقرمش وبطاطا محمّلة"),
            ("עוף וקריספי", "כנפיים, עוף פריך וצ'יפס מושחת"));
        var burgers = Cat("smash-burgers", "🍔", 2,
            ("Smash Burgers", "Smashed beef burgers & toppings"),
            ("سماش برغر", "برغر لحم مهروس وإضافات"),
            ("סמאש בורגר", "המבורגרים סמאש ותוספות"));
        var salads = Cat("salads", "🥗", 3,
            ("Salads", "Fresh, vibrant salads"),
            ("سلطات", "سلطات طازجة وملوّنة"),
            ("סלטים", "סלטים טריים וצבעוניים"));
        var pasta = Cat("pasta-ravioli", "🍝", 4,
            ("Pasta & Ravioli", "Fresh pasta, ravioli & gratins"),
            ("باستا ورافيولي", "باستا ورافيولي وأطباق بالكريمة"),
            ("פסטה ורביולי", "פסטה, רביולי ומוקרמים"));
        var sandwiches = Cat("sandwiches-platters", "🥙", 5,
            ("Sandwiches & Platters", "Pita, baguettes & platters"),
            ("ساندويتشات وأطباق", "بيتا، باغيت وصحون"),
            ("כריכים ומגשים", "פיתות, באגטים ומגשים"));
        var combo = Cat("viva-combo", "📦", 6,
            ("Viva Combo", "Our signature sharing box"),
            ("كومبو فيفا", "بوكس المشاركة المميّز"),
            ("ויווה קומבו", "בוקס הדגל לשיתוף"));
        var cocktails = Cat("cocktails", "🍹", 7,
            ("Cocktails", "Refreshing fruit cocktails"),
            ("كوكتيلات", "كوكتيلات فواكه منعشة"),
            ("קוקטיילים", "קוקטיילים פירותיים מרעננים"));
        var drinks = Cat("soft-drinks", "🥤", 8,
            ("Soft Drinks", "Sodas & cold drinks"),
            ("مشروبات", "مشروبات غازية وباردة"),
            ("משקאות", "מוגזים ומשקאות קרים"));
        db.Categories.AddRange(chicken, burgers, salads, pasta, sandwiches, combo, cocktails, drinks);

        db.MenuItems.AddRange(
            // ---------- Chicken & Crispy ----------
            Item("chili-wings", chicken.Id, 50, DietaryTag.Spicy, Allergen.None, "chicken,wings", 1,
                ("Chili Wings", "Crispy wings tossed in hot or sweet chili sauce"),
                ("أجنحة بالشيلي", "أجنحة مقرمشة بصلصة شيلي حارة أو حلوة"),
                ("כנפיים ברוטב צ'ילי", "כנפיים פריכות ברוטב צ'ילי חריף או מתוק"), popular: true),
            Item("crispy-chicken-wings-mix", chicken.Id, 55, DietaryTag.None, Allergen.Gluten, "fried,chicken", 2,
                ("Crispy Chicken & Wings Mix", "A mix of crispy chicken strips and wings"),
                ("ميكس كريسبي تشيكن وأجنحة", "تشكيلة من ستربس الدجاج المقرمش والأجنحة"),
                ("מיקס קריספי צ'יקן וכנפיים", "שילוב של רצועות עוף פריכות וכנפיים")),
            Item("crispy-chicken", chicken.Id, 60, DietaryTag.None, Allergen.Gluten, "crispy,chicken", 3,
                ("Crispy Chicken", "Golden crispy chicken served with fries"),
                ("كريسبي تشيكن", "دجاج مقرمش ذهبي يُقدّم مع البطاطا المقلية"),
                ("קריספי צ'יקן", "עוף פריך וזהוב מוגש עם צ'יפס"), popular: true),
            Item("loaded-fries-chicken", chicken.Id, 60, DietaryTag.Spicy, Allergen.Gluten | Allergen.Dairy, "loaded,fries", 4,
                ("Loaded Fries – Crispy Chicken", "Fries with crispy chicken, cheddar, jalapeño & house sauce"),
                ("بطاطا محمّلة – كريسبي تشيكن", "بطاطا مع دجاج مقرمش، شيدر، هالبينو وصلصة البيت"),
                ("צ'יפס מושחת – קריספי צ'יקן", "צ'יפס עם קריספי צ'יקן, צ'דר, חלפיניו ורוטב הבית")),
            Item("loaded-fries-onion", chicken.Id, 52, DietaryTag.Spicy | DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "cheese,fries", 5,
                ("Loaded Fries – Caramelized Onion", "Fries with caramelized onion, cheddar, tomato salsa, jalapeño & house sauce"),
                ("بطاطا محمّلة – بصل مكرمل", "بطاطا مع بصل مكرمل، شيدر، صلصة طماطم، هالبينو وصلصة البيت"),
                ("צ'יפס מושחת – בצל מקורמל", "צ'יפס עם בצל מקורמל, צ'דר, סלסת עגבניות, חלפיניו ורוטב הבית")),
            Item("asian-stir-fry-veg", chicken.Id, 58, DietaryTag.Vegetarian, Allergen.Soy, "stirfry,vegetables", 6,
                ("Asian Stir-Fry – Vegetarian", "Wok-tossed vegetables in Asian sauce"),
                ("مقلي آسيوي – نباتي", "خضار مقليّة بالووك مع صلصة آسيوية"),
                ("מוקפץ אסייתי צמחוני", "ירקות מוקפצים בוואק ברוטב אסייתי")),
            Item("asian-stir-fry-chicken", chicken.Id, 68, DietaryTag.None, Allergen.Soy | Allergen.Gluten, "stirfry,chicken", 7,
                ("Asian Stir-Fry – Chicken", "Wok-tossed vegetables with crispy chicken"),
                ("مقلي آسيوي – دجاج", "خضار مقليّة مع دجاج مقرمش"),
                ("מוקפץ אסייתי ירקות וקריספי צ'יקן", "ירקות מוקפצים עם קריספי צ'יקן")),
            Item("crispy-wings-12", chicken.Id, 50, DietaryTag.None, Allergen.Gluten, "chicken,wings", 8,
                ("Crispy Wings (12 pcs)", "A dozen golden crispy chicken wings"),
                ("أجنحة مقرمشة (12 قطعة)", "اثنتا عشرة قطعة من أجنحة الدجاج المقرمشة"),
                ("קריספי כנפיים 12 יח'", "תריסר כנפיים פריכות וזהובות")),
            Item("family-strips-wings", chicken.Id, 150, DietaryTag.None, Allergen.Gluten, "chicken,platter", 9,
                ("Family Strips & Wings", "Chicken strips & wings with salad, fries and a large drink"),
                ("وجبة عائلية ستربس وأجنحة", "ستربس دجاج وأجنحة مع سلطة، بطاطا ومشروب كبير"),
                ("מנה משפחתית סטריפס וכנפיים", "סטריפס וכנפיים עם סלט, צ'יפס ושתייה גדולה"), recommended: true),

            // ---------- Smash Burgers ----------
            Item("smash-120", burgers.Id, 50, DietaryTag.None, Allergen.Gluten | Allergen.Dairy, "smash,burger", 10,
                ("Smash Burger 120g", "Juicy smashed beef patty in a soft bun"),
                ("سماش برغر 120غ", "قطعة لحم بقري مهروسة وعصيرية داخل خبز طري"),
                ("סמאש בורגר 120 גרם", "קציצת בקר עסיסית בלחמנייה רכה")),
            Item("smash-240", burgers.Id, 65, DietaryTag.None, Allergen.Gluten | Allergen.Dairy, "burger,cheese", 11,
                ("Smash Burger 240g", "Double smashed beef patties"),
                ("سماش برغر 240غ", "قطعتا لحم بقري مهروستان"),
                ("סמאש בורגר 240 גרם", "שתי קציצות בקר עסיסיות"), popular: true),
            Item("smash-360", burgers.Id, 80, DietaryTag.None, Allergen.Gluten | Allergen.Dairy, "double,burger", 12,
                ("Smash Burger 360g", "Triple smashed beef patties for big appetites"),
                ("سماش برغر 360غ", "ثلاث قطع لحم بقري للجوعى"),
                ("סמאש בורגר 360 גרם", "שלוש קציצות בקר לרעבים במיוחד")),
            Item("add-fried-egg", burgers.Id, 5, DietaryTag.Vegetarian, Allergen.Eggs, "fried,egg", 13,
                ("Add: Fried Egg", "Burger topping"),
                ("إضافة: بيضة عيون", "إضافة للبرغر"),
                ("תוספת: ביצת עין", "תוספת לבורגר")),
            Item("add-mushrooms", burgers.Id, 5, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "mushrooms", 14,
                ("Add: Mushrooms", "Burger topping"),
                ("إضافة: فطر", "إضافة للبرغر"),
                ("תוספת: פטריות", "תוספת לבורגר")),
            Item("add-caramelized-onion", burgers.Id, 5, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "caramelized,onion", 15,
                ("Add: Caramelized Onion", "Burger topping"),
                ("إضافة: بصل مكرمل", "إضافة للبرغر"),
                ("תוספת: בצל מקורמל", "תוספת לבורגר")),
            Item("add-cheddar", burgers.Id, 5, DietaryTag.Vegetarian, Allergen.Dairy, "cheddar,cheese", 16,
                ("Add: Cheddar", "Burger topping"),
                ("إضافة: جبنة شيدر", "إضافة للبرغر"),
                ("תוספת: גבינת צ'דר", "תוספת לבורגר")),
            Item("add-extra-cheddar", burgers.Id, 10, DietaryTag.Vegetarian, Allergen.Dairy, "cheese", 17,
                ("Add: Extra Cheddar", "Double cheddar topping"),
                ("إضافة: شيدر إضافي", "جبنة شيدر مضاعفة"),
                ("תוספת: אקסטרה צ'דר", "תוספת צ'דר כפולה")),

            // ---------- Salads ----------
            Item("fattoush", salads.Id, 57, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.Gluten, "fattoush,salad", 18,
                ("Fattoush Salad", "Crisp greens, toasted pita, sumac & lemon dressing"),
                ("سلطة فتوش", "خضار طازجة، خبز محمّص، سمّاق وصلصة الليمون"),
                ("סלט פטוש", "ירקות פריכים, פיתה קלויה, סומאק ורוטב לימון")),
            Item("caesar", salads.Id, 45, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy | Allergen.Eggs, "caesar,salad", 19,
                ("Caesar Salad", "Romaine, croutons & parmesan in Caesar dressing — add arancini or chicken +10₪"),
                ("سلطة سيزر", "خس، كروتون وبارميزان مع صلصة السيزر — إضافة أرانتشيني أو دجاج +10₪"),
                ("סלט קיסר", "חסה, קרוטונים ופרמזן ברוטב קיסר — תוספת ארנצ'יני או עוף +10₪"), popular: true),
            Item("halloumi-salad", salads.Id, 62, DietaryTag.Vegetarian, Allergen.Dairy, "halloumi,salad", 20,
                ("Halloumi Salad", "Fresh greens with grilled halloumi cheese"),
                ("سلطة حلومي", "خضار طازجة مع جبنة الحلوم المشوية"),
                ("סלט חלומי", "ירקות טריים עם גבינת חלומי צלויה"), popular: true),
            Item("tabbouleh", salads.Id, 47, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.Gluten, "tabbouleh", 21,
                ("Tabbouleh Salad", "Parsley, bulgur, tomato, mint & lemon"),
                ("سلطة تبولة", "بقدونس، برغل، طماطم، نعناع وليمون"),
                ("סלט טבולה", "פטרוזיליה, בורגול, עגבנייה, נענע ולימון")),
            Item("caprese-salad", salads.Id, 65, DietaryTag.Vegetarian | DietaryTag.GlutenFree, Allergen.Dairy, "caprese,salad", 22,
                ("Caprese Salad", "Mozzarella, tomato, basil & olive oil"),
                ("سلطة كابريزي", "موزاريلا، طماطم، ريحان وزيت زيتون"),
                ("סלט קפרזה", "מוצרלה, עגבנייה, בזיליקום ושמן זית")),

            // ---------- Pasta & Ravioli ----------
            Item("pasta-alfredo", pasta.Id, 58, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "alfredo,pasta", 23,
                ("Pasta Alfredo", "Pasta in a creamy parmesan Alfredo sauce"),
                ("باستا ألفريدو", "باستا بصلصة ألفريدو الكريمية بالبارميزان"),
                ("פסטה ברוטב אלפרדו", "פסטה ברוטב אלפרדו שמנתי עם פרמזן")),
            Item("pasta-rose", pasta.Id, 58, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "pink,pasta", 24,
                ("Pasta Rosé", "Pasta in a tomato-cream rosé sauce"),
                ("باستا روزيه", "باستا بصلصة الطماطم والكريمة الوردية"),
                ("פסטה ברוטב רוזה", "פסטה ברוטב רוזה של עגבניות ושמנת")),
            Item("pasta-pesto", pasta.Id, 58, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "pesto,pasta", 25,
                ("Pasta Pesto", "Pasta tossed in fresh basil pesto"),
                ("باستا بيستو", "باستا بصلصة البيستو والريحان الطازج"),
                ("פסטה ברוטב פסטו", "פסטה ברוטב פסטו של בזיליקום טרי")),
            Item("ravioli-sweet-potato", pasta.Id, 60, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy | Allergen.Eggs, "ravioli", 26,
                ("Sweet Potato Ravioli", "Ravioli filled with sweet potato"),
                ("رافيولي بطاطا حلوة", "رافيولي محشي بالبطاطا الحلوة"),
                ("רביולי בטטה", "רביולי במילוי בטטה")),
            Item("ravioli-four-cheese", pasta.Id, 60, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy | Allergen.Eggs, "ravioli,cheese", 27,
                ("Four Cheese Ravioli", "Ravioli filled with four cheeses"),
                ("رافيولي أربع أجبان", "رافيولي محشي بأربعة أنواع جبن"),
                ("רביולי ארבע גבינות", "רביולי במילוי ארבע גבינות")),
            Item("ravioli-ricotta-mushroom", pasta.Id, 60, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy | Allergen.Eggs, "ravioli,mushroom", 28,
                ("Ricotta & Mushroom Ravioli", "Ravioli filled with ricotta and mushrooms"),
                ("رافيولي ريكوتا وفطر", "رافيولي محشي بالريكوتا والفطر"),
                ("רביולי ריקוטה פטריות", "רביולי במילוי ריקוטה ופטריות")),
            Item("green-spaghetti-chicken", pasta.Id, 70, DietaryTag.None, Allergen.Gluten | Allergen.Dairy, "spaghetti,pesto", 29,
                ("Green Spaghetti, Chicken & Pesto Butter", "Spinach spaghetti with chicken breast in pesto butter"),
                ("سباغيتي خضراء بصدر دجاج وزبدة البيستو", "سباغيتي سبانخ مع صدر دجاج بزبدة البيستو"),
                ("ספגטי ירוקות, חזה עוף בחמאת פסטו", "ספגטי תרד עם חזה עוף בחמאת פסטו"), recommended: true),
            Item("arancini", pasta.Id, 58, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "arancini", 30,
                ("Arancini", "Crispy fried risotto balls"),
                ("أرانتشيني", "كرات أرز ريزوتو مقلية ومقرمشة"),
                ("ארנצ'יני", "כדורי ריזוטו מטוגנים ופריכים")),
            Item("crispy-lasagna-fingers", pasta.Id, 60, DietaryTag.Vegetarian, Allergen.Gluten | Allergen.Dairy, "lasagna", 31,
                ("Crispy Lasagna Fingers", "Fried lasagna sticks, crispy outside and cheesy inside"),
                ("أصابع لازانيا مقرمشة", "أصابع لازانيا مقلية، مقرمشة من الخارج وبالجبن من الداخل"),
                ("אצבעות לזניה קריספי", "אצבעות לזניה מטוגנות, פריכות מבחוץ וגבינתיות מבפנים")),
            Item("creamy-schnitzel-sweet-potato", pasta.Id, 68, DietaryTag.None, Allergen.Gluten | Allergen.Dairy | Allergen.Eggs, "schnitzel", 32,
                ("Creamy Schnitzel, Sweet Potato & Chestnuts", "Schnitzel gratin with sweet potato and chestnuts"),
                ("شنيتسل بالكريمة مع بطاطا حلوة وكستناء", "شنيتسل بالكريمة مع البطاطا الحلوة والكستناء"),
                ("מוקרם שניצל בטטה וערמונים", "שניצל מוקרם עם בטטה וערמונים")),
            Item("creamy-mushroom-halloumi", pasta.Id, 58, DietaryTag.Vegetarian, Allergen.Dairy, "mushrooms,cream", 33,
                ("Creamy Mushrooms, Sweet Potato & Halloumi", "Creamy mushrooms and sweet potato topped with halloumi"),
                ("فطر وبطاطا حلوة بالكريمة مع حلومي", "فطر وبطاطا حلوة بالكريمة مع جبنة الحلوم"),
                ("פטריות ובטטה מוקרמות עם חלומי", "פטריות ובטטה מוקרמות בתוספת גבינת חלומי")),

            // ---------- Sandwiches & Platters ----------
            Item("schnitzel-platter", sandwiches.Id, 60, DietaryTag.None, Allergen.Gluten | Allergen.Eggs, "schnitzel,platter", 34,
                ("Schnitzel / Mixed Platter", "Schnitzel or mixed platter with coleslaw and pitas"),
                ("صحن شنيتسل / مشكّل", "شنيتسل أو مشكّل مع سلطة الملفوف والخبز"),
                ("חמגשית שניצל/מעורב", "שניצל או מעורב עם סלט כרוב ופיתות")),
            Item("falafel-platter", sandwiches.Id, 46, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.Sesame | Allergen.Gluten, "falafel", 35,
                ("Falafel Platter (14)", "14 falafel balls with salad and pitas"),
                ("صحن فلافل (14)", "14 قطعة فلافل مع سلطة وخبز"),
                ("פלאפל (14 כדור)", "14 כדורי פלאפל עם סלט ופיתות"), popular: true),
            Item("falafel-pita", sandwiches.Id, 22, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.Sesame | Allergen.Gluten, "falafel,pita", 36,
                ("Falafel Pita", "Pita filled with falafel and salad"),
                ("بيتا فلافل", "خبز بيتا محشي بالفلافل والسلطة"),
                ("פיתה פלאפל", "פיתה במילוי פלאפל וסלט")),
            Item("schnitzel-pita", sandwiches.Id, 27, DietaryTag.None, Allergen.Gluten | Allergen.Eggs, "schnitzel,pita", 37,
                ("Schnitzel Pita", "Pita filled with crispy schnitzel"),
                ("بيتا شنيتسل", "خبز بيتا محشي بالشنيتسل المقرمش"),
                ("פיתה שניצל", "פיתה במילוי שניצל פריך")),
            Item("schnitzel-baguette", sandwiches.Id, 45, DietaryTag.None, Allergen.Gluten | Allergen.Eggs, "schnitzel,baguette", 38,
                ("Schnitzel Baguette", "Baguette filled with crispy schnitzel"),
                ("باغيت شنيتسل", "باغيت محشي بالشنيتسل المقرمش"),
                ("בגט שניצל", "באגט במילוי שניצל פריך")),
            Item("mixed-baguette", sandwiches.Id, 45, DietaryTag.None, Allergen.Gluten, "baguette,sandwich", 39,
                ("Mixed Baguette", "Baguette with a mixed filling"),
                ("باغيت مشكّل", "باغيت بحشوة مشكّلة"),
                ("בגט מעורב", "באגט במילוי מעורב")),

            // ---------- Viva Combo ----------
            Item("viva-combo-box", combo.Id, 230, DietaryTag.None, Allergen.Gluten | Allergen.Dairy, "combo,food", 40,
                ("Viva Combo Box", "Our signature sharing box — a generous selection of Viva favorites"),
                ("بوكس فيفا كومبو", "بوكس المشاركة المميّز — تشكيلة كبيرة من أطباق فيفا المفضّلة"),
                ("ויווה קומבו בוקס", "בוקס הדגל לשיתוף — מבחר נדיב מהמנות האהובות של ויווה"), popular: true, recommended: true),

            // ---------- Cocktails ----------
            Item("cocktail-blueberry-watermelon-passion", cocktails.Id, 18, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "cocktail", 41,
                ("Blueberry, Watermelon & Passionfruit", "Refreshing fruit cocktail"),
                ("توت أزرق، بطيخ وباشن فروت", "كوكتيل فواكه منعش"),
                ("בלוברי, אבטיח ופסיפלורה", "קוקטייל פירות מרענן"), popular: true),
            Item("cocktail-watermelon-passion", cocktails.Id, 18, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "watermelon,juice", 42,
                ("Watermelon & Passionfruit", "Refreshing fruit cocktail"),
                ("بطيخ وباشن فروت", "كوكتيل فواكه منعش"),
                ("אבטיח ופסיפלורה", "קוקטייל פירות מרענן")),
            Item("cocktail-lemon-mint-passion", cocktails.Id, 18, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "lemonade,mint", 43,
                ("Lemon-Mint & Passionfruit", "Zesty lemon, mint and passionfruit"),
                ("ليمون نعناع وباشن فروت", "ليمون منعش، نعناع وباشن فروت"),
                ("למון נענע ופסיפלורה", "לימון מרענן, נענע ופסיפלורה")),
            Item("cocktail-blueberry-lemon-mint", cocktails.Id, 18, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "blueberry,drink", 44,
                ("Blueberry & Lemon-Mint", "Blueberry with lemon and mint"),
                ("توت أزرق وليمون نعناع", "توت أزرق مع ليمون ونعناع"),
                ("בלוברי ולמון נענע", "בלוברי עם לימון ונענע")),
            Item("cocktail-blueberry-watermelon", cocktails.Id, 18, DietaryTag.Vegan | DietaryTag.Vegetarian, Allergen.None, "berry,cocktail", 45,
                ("Blueberry & Watermelon", "Blueberry and watermelon cooler"),
                ("توت أزرق وبطيخ", "مشروب التوت الأزرق والبطيخ"),
                ("בלוברי ואבטיח", "משקה בלוברי ואבטיח")),

            // ---------- Soft Drinks ----------
            Item("cola-large", drinks.Id, 15, DietaryTag.None, Allergen.None, "cola", 46,
                ("Cola (Large)", ""), ("كولا (كبير)", ""), ("קולה (גדול)", "")),
            Item("cola-zero-large", drinks.Id, 15, DietaryTag.None, Allergen.None, "cola", 47,
                ("Cola Zero (Large)", ""), ("كولا زيرو (كبير)", ""), ("קולה זירו (גדול)", "")),
            Item("cola-small", drinks.Id, 7, DietaryTag.None, Allergen.None, "cola,glass", 48,
                ("Cola (Small)", ""), ("كولا (صغير)", ""), ("קולה (קטן)", "")),
            Item("cola-zero-small", drinks.Id, 7, DietaryTag.None, Allergen.None, "cola,can", 49,
                ("Cola Zero (Small)", ""), ("كولا زيرو (صغير)", ""), ("קולה זירו (קטן)", "")),
            Item("grape-soda", drinks.Id, 7, DietaryTag.None, Allergen.None, "grape,juice", 50,
                ("Grape Soda", ""), ("مشروب العنب", ""), ("ענבים", "")),
            Item("soda", drinks.Id, 7, DietaryTag.None, Allergen.None, "soda,water", 51,
                ("Soda", ""), ("صودا", ""), ("סודה", "")),
            Item("xl-ten", drinks.Id, 7, DietaryTag.None, Allergen.None, "energy,drink", 52,
                ("XL TEN", ""), ("XL TEN", ""), ("XL TEN", "")),
            Item("xl", drinks.Id, 7, DietaryTag.None, Allergen.None, "energy,drink", 53,
                ("XL", ""), ("XL", ""), ("XL", ""))
        );

        // ---- Home banner ----
        db.Banners.Add(new Banner
        {
            ImageUrl = "https://loremflickr.com/1600/800/restaurant,food?lock=900",
            SortOrder = 1,
            IsActive = true,
            Translations =
            [
                new() { Language = Language.English, Title = "Benvenuti a Viva Italia", Subtitle = "Crispy, fresh & made with love" },
                new() { Language = Language.Arabic, Title = "أهلاً بكم في فيفا إيطاليا", Subtitle = "مقرمش، طازج ومصنوع بحب" },
                new() { Language = Language.Hebrew, Title = "ברוכים הבאים לויווה איטליה", Subtitle = "פריך, טרי ועשוי באהבה" },
            ],
        });

        await db.SaveChangesAsync();
    }

    static string Img(string keywords, int lockId) => $"https://loremflickr.com/600/450/{keywords}?lock={lockId}";

    static Category Cat(string slug, string icon, int order,
        (string Name, string Desc) en, (string Name, string Desc) ar, (string Name, string Desc) he) => new()
    {
        Slug = slug,
        Icon = icon,
        SortOrder = order,
        Translations =
        [
            new() { Language = Language.English, Name = en.Name, Description = en.Desc },
            new() { Language = Language.Arabic, Name = ar.Name, Description = ar.Desc },
            new() { Language = Language.Hebrew, Name = he.Name, Description = he.Desc },
        ],
    };

    static MenuItem Item(string slug, Guid catId, decimal price, DietaryTag tags, Allergen allergens,
        string imgKeywords, int lockId,
        (string Name, string Desc) en, (string Name, string Desc) ar, (string Name, string Desc) he,
        bool popular = false, bool recommended = false) => new()
    {
        Slug = slug,
        CategoryId = catId,
        Price = price,
        IsPopular = popular,
        IsRecommended = recommended,
        DietaryTags = tags,
        Allergens = allergens,
        Images = [new() { Url = Img(imgKeywords, lockId), IsPrimary = true }],
        Translations =
        [
            new() { Language = Language.English, Name = en.Name, Description = en.Desc },
            new() { Language = Language.Arabic, Name = ar.Name, Description = ar.Desc },
            new() { Language = Language.Hebrew, Name = he.Name, Description = he.Desc },
        ],
    };
}
