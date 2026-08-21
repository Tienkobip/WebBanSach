// JavaScript for Login component
// Components/Pages/Auth/Login.razor.js

export async function submitLoginForm(formElementId) {
    const form = document.getElementById(formElementId);

    try {
        const response = await fetch('/api/management/login', {
            method: 'POST',
            body: new FormData(form)
        });

        if (response.ok) {
            return { success: true, message: "" };
        }

        // Đọc nội dung lỗi trả về
        const errorData = await response.json();

        return { success: false, message: errorData.message };
    }
    catch (err) {
        return { success: false, message: "Không thể kết nối đến máy chủ." };
    }
}