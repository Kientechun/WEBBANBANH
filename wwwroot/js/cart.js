function csrfToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

async function postCart(action, productId, quantity) {
    const body = new URLSearchParams({ productId, quantity, __RequestVerificationToken: csrfToken() });
    const response = await fetch('/Cart/' + action, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
    let result = {};
    try { result = await response.json(); } catch { }
    if (!response.ok) throw new Error(result.message || 'Không thể cập nhật giỏ hàng.');
    document.querySelectorAll('.cart-count-badge').forEach(b => b.textContent = result.count);
    return result;
}

function showMessage(el, text, ok) {
    if (!el) return;
    el.textContent = text;
    el.classList.toggle('text-danger', !ok);
    el.classList.toggle('text-success', ok);
}

// Trang giỏ hàng: đổi số lượng / xóa
document.querySelectorAll('.cart-quantity').forEach(input => input.addEventListener('change', async () => {
    try { await postCart('SetQuantity', input.dataset.id, input.value); location.reload(); }
    catch (error) { showMessage(document.querySelector('#cart-message'), error.message, false); }
}));
document.querySelectorAll('.remove-cart').forEach(button => button.addEventListener('click', async () => {
    try { await postCart('SetQuantity', button.dataset.id, 0); location.reload(); }
    catch (error) { showMessage(document.querySelector('#cart-message'), error.message, false); }
}));

// Trang chi tiết: thêm vào giỏ (cộng dồn)
document.querySelectorAll('.add-cart-form').forEach(form => form.addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(form);
    const msg = form.querySelector('.cart-message');
    try { const r = await postCart('Add', data.get('productId'), data.get('quantity')); showMessage(msg, r.message, true); }
    catch (error) { showMessage(msg, error.message, false); }
}));

// Thẻ sản phẩm: nút "+ Giỏ"
document.querySelectorAll('.quick-add').forEach(button => button.addEventListener('click', async () => {
    const msg = button.closest('.card-body')?.querySelector('.quick-msg');
    try { const r = await postCart('Add', button.dataset.id, 1); showMessage(msg, r.message, true); }
    catch (error) { showMessage(msg, error.message, false); }
}));
