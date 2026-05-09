let inactivityTimer;
function resetInactivityTimer() {

    clearTimeout(inactivityTimer);

    inactivityTimer = setTimeout(function () {

        alert("Phiên đăng nhập đã hết hạn do không hoạt động trong 10 phút.");

        window.location.href = "/Authentication/Logout";

    }, 10 * 60 * 1000);
}
window.onload = resetInactivityTimer;
document.onmousemove = resetInactivityTimer;
document.onkeypress = resetInactivityTimer;
document.onclick = resetInactivityTimer;
document.onscroll = resetInactivityTimer;