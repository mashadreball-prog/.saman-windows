# اسکلت سامان ویندوز (کامپایل‌نشده)
این اسکلت در محیط بدون .NET نوشته شده و هنوز کامپایل نشده است. در Visual Studio 2019/2022 پوشه را باز کنید، سه پروژه را به Solution اضافه کنید (Add Existing Project) و Restore بزنید.
- Saman.Core: مدل‌ها و تحلیلگر پذیرش هوشمند
- Saman.Data: اسکریپت دیتابیس Schema_v1.sql (SQLite)، Dapper
- Saman.UI: WPF، تم سبز و مسی

## تست روی ویندوز ۷ (۶۴ بیتی، SP1، رم ۳ گیگ)
نصب‌های لازم روی لپ‌تاپ: .NET Framework 4.8 (با به‌روزرسانی‌های SHA-2 مثل KB4474419 و KB4490628) و Visual C++ Redistributable 2015-2019 (x64).
بیلد را روی سیستم جدیدتر با Visual Studio بگیرید (Release, x64) و پوشه bin\Release را کامل به لپ‌تاپ ببرید.

## بیلد با GitHub Actions
1. مخزن Private بسازید و کل محتوای این پوشه (شامل .github و .gitignore) را آپلود کنید.
2. تب Actions > Build Saman Windows > Run workflow.
3. پس از پایان، از پایین صفحه اجرا Artifact با نام Saman-Windows-Release را دانلود کنید. فایل build_log.txt داخلش است؛ اگر بیلد خطا داد آن را برای من بفرستید.
