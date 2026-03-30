# Script ?? thay th? toàn b? chu?i không d?u thành ti?ng Vi?t có d?u trong file .cshtml

$viewsPath = ".\SV22T1020590.Admin\Views"
$replacements = @{
    "Quan ly Nha cung cap" = "Qu?n lý Nhà cung c?p"
    "Quan ly Khach hang" = "Qu?n lý Khách hàng"
    "Quan ly Nhan vien" = "Qu?n lý Nhân viên"
    "Quan ly Nguoi giao hang" = "Qu?n lý Ng??i giao hàng"
    "Quan ly Loai Hang" = "Qu?n lý Lo?i Hàng"
    "Quan ly Mat hang" = "Qu?n lý M?t hàng"
    "Quan ly Don hang" = "Qu?n lý ??n hàng"
    "Lap don hang" = "L?p ??n hàng"
    "Ten nha cung cap" = "Tên nhà cung c?p"
    "Ten khach hang" = "Tên khách hàng"
    "Ten nhan vien" = "Tên nhân viên"
    "Ten loai hang" = "Tên lo?i hàng"
    "Ten giao dich" = "Tên giao d?ch"
    "Dien thoai" = "?i?n tho?i"
    "Dia chi" = "??a ch?"
    "Tinh / Thanh" = "T?nh / Thành"
    "Thao tac" = "Thao tác"
    "Bo sung" = "B? sung"
    "Luu du lieu" = "L?u d? li?u"
    "Quay lai" = "Quay l?i"
    "Khong co du lieu" = "Không có d? li?u"
    "Danh sach nay co" = "Danh sách này có"
    "nha cung cap" = "nhà cung c?p"
    "khach hang" = "khách hàng"
    "nhan vien" = "nhân viên"
    "loai hang" = "lo?i hàng"
    "mat hang" = "m?t hàng"
    "don hang" = "??n hàng"
    "giao dich" = "giao d?ch"
    "Trang thai" = "Tr?ng thái"
    "Da khoa" = "?ã khóa"
    "Hoat dong" = "Ho?t ??ng"
    "Xoa khach hang" = "Xóa khách hàng"
    "Xoa nha cung cap" = "Xóa nhà cung c?p"
    "Xoa nhan vien" = "Xóa nhân viên"
    "Xoa nguoi giao hang" = "Xóa ng??i giao hàng"
    "Xoa loai hang" = "Xóa lo?i hàng"
    "Xoa mat hang" = "Xóa m?t hàng"
    "Xoa don hang" = "Xóa ??n hàng"
    "Thong tin nha cung cap" = "Thông tin nhà cung c?p"
    "Thong tin khach hang" = "Thông tin khách hàng"
    "Thong tin nhan vien" = "Thông tin nhân viên"
    "Thong tin nguoi giao hang" = "Thông tin ng??i giao hàng"
    "Thong tin loai hang" = "Thông tin lo?i hàng"
    "Thong tin mat hang" = "Thông tin m?t hàng"
    "Thong tin don hang" = "Thông tin ??n hàng"
    "Ho va ten" = "H? và tên"
    "Ngay sinh" = "Ngày sinh"
    "Dang lam viec" = "?ang làm vi?c"
    "Cap nhat san pham" = "C?p nh?t s?n ph?m"
    "So luong" = "S? l??ng"
    "San pham" = "S?n ph?m"
}

Get-ChildItem -Path $viewsPath -Filter "*.cshtml" -Recurse | ForEach-Object {
    $filePath = $_.FullName
    $content = Get-Content -Path $filePath -Encoding UTF8
    $modified = $false
    
    foreach ($key in $replacements.Keys) {
        if ($content -like "*$key*") {
            $content = $content -replace [regex]::Escape($key), $replacements[$key]
            $modified = $true
        }
    }
    
    if ($modified) {
        Set-Content -Path $filePath -Value $content -Encoding UTF8
        Write-Host "Updated: $filePath"
    }
}

Write-Host "Hoàn t?t c?p nh?t t?t c? file .cshtml"
