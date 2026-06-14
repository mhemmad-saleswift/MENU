using Restaurant.Domain;

namespace Restaurant.Web.Services;

/// <summary>Static UI string catalog for the three supported languages.</summary>
public static class UiText
{
    static readonly Dictionary<string, (string En, string Ar, string He)> Map = new()
    {
        ["brand"] = ("Viva Italia", "Viva Italia", "Viva Italia"),
        ["welcome.prompt"] = ("Choose your language", "اختر لغتك", "בחר את השפה"),
        ["welcome.tagline"] = ("Authentic Italian, crafted with passion", "إيطالي أصيل، مصنوع بشغف", "איטלקי אותנטי, נוצר באהבה"),
        ["nav.home"] = ("Home", "الرئيسية", "בית"),
        ["nav.menu"] = ("Menu", "القائمة", "תפריט"),
        ["nav.admin"] = ("Admin", "الإدارة", "ניהול"),
        ["hero.cta"] = ("Explore the Menu", "استكشف القائمة", "גלו את התפריט"),
        ["home.cta.menu"] = ("Move to Menu", "انتقل إلى القائمة", "עבור לתפריט"),
        ["home.featured"] = ("Chef's Featured", "اختيارات الشيف", "מומלצי השף"),
        ["home.categories"] = ("Browse Categories", "تصفح الأقسام", "עיון בקטגוריות"),
        ["home.popular"] = ("Most Loved", "الأكثر طلباً", "האהובים ביותר"),
        ["menu.title"] = ("Our Menu", "قائمتنا", "התפריט שלנו"),
        ["menu.all"] = ("All", "الكل", "הכל"),
        ["menu.search"] = ("Search dishes…", "ابحث عن الأطباق…", "חיפוש מנות…"),
        ["menu.filters"] = ("Filters", "تصفية", "סינון"),
        ["menu.price"] = ("Price range", "نطاق السعر", "טווח מחירים"),
        ["menu.dietary"] = ("Dietary", "النظام الغذائي", "תזונה"),
        ["menu.available"] = ("Available only", "المتوفر فقط", "זמין בלבד"),
        ["menu.popular"] = ("Popular only", "الشائع فقط", "פופולרי בלבד"),
        ["menu.noresults"] = ("No dishes match your search.", "لا توجد أطباق مطابقة.", "אין מנות תואמות."),
        ["menu.clear"] = ("Clear", "مسح", "נקה"),
        ["item.ingredients"] = ("Ingredients", "المكونات", "מרכיבים"),
        ["item.allergens"] = ("Allergens", "مسببات الحساسية", "אלרגנים"),
        ["item.viewar"] = ("View in AR", "عرض بالواقع المعزز", "צפייה ב-AR"),
        ["item.unavailable"] = ("Currently unavailable", "غير متوفر حالياً", "לא זמין כעת"),
        ["item.back"] = ("Back to menu", "العودة للقائمة", "חזרה לתפריט"),
        ["badge.popular"] = ("Popular", "شائع", "פופולרי"),
        ["badge.recommended"] = ("Chef's Pick", "اختيار الشيف", "בחירת השף"),
        ["badge.new"] = ("New", "جديد", "חדש"),
        ["tag.vegan"] = ("Vegan", "نباتي صرف", "טבעוני"),
        ["tag.vegetarian"] = ("Vegetarian", "نباتي", "צמחוני"),
        ["tag.glutenfree"] = ("Gluten-Free", "خالٍ من الغلوتين", "ללא גלוטן"),
        ["tag.spicy"] = ("Spicy", "حار", "חריף"),
        ["tag.halal"] = ("Halal", "حلال", "כשר"),
        ["tag.dairyfree"] = ("Dairy-Free", "خالٍ من الألبان", "ללא חלב"),
        ["tag.nutfree"] = ("Nut-Free", "خالٍ من المكسرات", "ללא אגוזים"),
        ["tag.organic"] = ("Organic", "عضوي", "אורגני"),
        // Admin
        ["admin.dashboard"] = ("Dashboard", "لوحة التحكم", "לוח בקרה"),
        ["admin.categories"] = ("Categories", "الأقسام", "קטגוריות"),
        ["admin.items"] = ("Items", "الأصناف", "פריטים"),
        ["admin.ai"] = ("AI Assistant", "المساعد الذكي", "עוזר AI"),
        ["admin.login"] = ("Sign in", "تسجيل الدخول", "התחברות"),
        ["admin.logout"] = ("Sign out", "تسجيل الخروج", "התנתקות"),
        ["admin.username"] = ("Username", "اسم المستخدم", "שם משתמש"),
        ["admin.password"] = ("Password", "كلمة المرور", "סיסמה"),
        ["common.save"] = ("Save", "حفظ", "שמירה"),
        ["common.cancel"] = ("Cancel", "إلغاء", "ביטול"),
        ["common.delete"] = ("Delete", "حذف", "מחיקה"),
        ["common.edit"] = ("Edit", "تعديل", "עריכה"),
        ["common.add"] = ("Add", "إضافة", "הוספה"),
        ["common.confirm"] = ("Confirm", "تأكيد", "אישור"),
    };

    public static string Get(string key, Language lang)
    {
        if (!Map.TryGetValue(key, out var v)) return key;
        return lang switch { Language.Arabic => v.Ar, Language.Hebrew => v.He, _ => v.En };
    }
}
