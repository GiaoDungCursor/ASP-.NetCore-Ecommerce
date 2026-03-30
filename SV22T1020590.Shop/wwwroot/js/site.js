// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // Intercept Add to Cart forms via event delegation
    $(document).on("submit", "form", function (e) {
        var form = $(this);
        var action = form.attr("action") || "";
        
        if (action.toLowerCase().indexOf("cart/add") >= 0) {
            e.preventDefault();
            
            var url = action || "/Cart/Add";
            var data = form.serialize();
            
            $.ajax({
                type: "POST",
                url: url,
                data: data,
                dataType: "json", // expecting JSON callback
                success: function (response) {
                    if (response.success) {
                        // Update the cart pills everywhere
                        $(".cart-pill").text("Giỏ (" + response.cartCount + ")");
                        
                        showToast(response.message, "bg-success");
                    } else {
                        showToast(response.message || "Lỗi khi thêm vào giỏ hàng.", "bg-danger");
                    }
                },
                error: function () {
                    showToast("Đã có lỗi xảy ra. Vui lòng thử lại.", "bg-danger");
                }
            });
        }
    });
});

function showToast(message, bgClass) {
    var toastEl = document.getElementById('shopToast');
    if (!toastEl) return;
    
    // Remove existing background classes
    toastEl.classList.remove('bg-success', 'bg-danger', 'bg-warning', 'bg-info');
    toastEl.classList.add(bgClass);
    
    document.getElementById('shopToastBody').innerText = message;
    
    var toast = new bootstrap.Toast(toastEl, { delay: 3000 });
    toast.show();
}
