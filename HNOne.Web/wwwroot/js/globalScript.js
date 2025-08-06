function closeMenuHandler() {
    // tắt menu khi chuyển trang khác
    var lstMenu = document.querySelectorAll('#navbar-menu .navbar-nav .nav-item.dropdown .dropdown-menu.show');
    if (lstMenu) {
        for (let i = 0; i < lstMenu.length; i++) {
            lstMenu[i].classList.remove('show')
        }
    }
}


function setActiveStyle(color) {
    const alternateStyles = document.querySelectorAll(".alternate-style");
    alternateStyles.forEach((style) => {
        if (color === style.getAttribute("title")) {
            style.removeAttribute("disabled");
        } else {
            style.setAttribute("disabled", "true");
        }
    });
}

// focus input
function focusInput(id) {
    var input = document.querySelector('input[name="'+id+'"]');
    if (input) {
        input.focus();
    }
}

// scroll đến section
function scrollToSection(id) {
    const section = document.getElementById(id);
    if (section) {
        if (id === "section-accordion-1") {
            section.scrollIntoView({ top: 0, behavior: "smooth" });
        }
        else {
            section.scrollIntoView({ behavior: "smooth" });
        }
    }
}

/*
hainguyen create 2024.07.17
desc: các tạo tác liên quan đến js cho Home controller thêm ở đây
+ thêm sự kiện double click vào ô chi tiết
*/
var onloadCallback = function () {
    HomeController.scriptLoaded = true;
}
HomeController = {
    scriptLoaded: null,
    dotNetObjReference: null,
    clickCount: 0,
    timeout: null,
    Init: function (dotNetObjReference) {
        HomeController.dotNetObjReference = dotNetObjReference;
        if (HomeController.scriptLoaded === true) {
            HomeController.Render();
        } else {
            HomeController.WaitForRender();
        }
    },
    WaitForRender: function () {
        if (HomeController.scriptLoaded === true) {
            HomeController.Render();
        } else {
            setTimeout(() => HomeController.WaitForRender(), 100);
        }
    },
    Render: function (day, month, year) {
        // bắt sự kiện dbclick mới trigger event. Vì Moblie không hiểu dblclick
        if (HomeController.timeout) {
            clearTimeout(HomeController.timeout);
        }
        HomeController.clickCount++;
        if (HomeController.clickCount === 2) {
            HomeController.dotNetObjReference.invokeMethodAsync('OpenPopupHandler', day, month, year);
            HomeController.clickCount = 0;
        }
        HomeController.timeout = setTimeout(() => {
            HomeController.clickCount = 0;
        }, 300);
    }
}

/* back to top button event */
document.addEventListener('DOMContentLoaded', function () {
    var btnScrollToTop = document.getElementById('btn-back-to-top');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 300) {
            btnScrollToTop.classList.add('show');
        } else {
            btnScrollToTop.classList.remove('show');
        }
    });
    btnScrollToTop.addEventListener('click', function (e) {
        e.preventDefault();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});

//Tải file từ stream
async function downloadFileFromStream(fileName, mimeType, contentStreamReference) {
    try {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        console.log(arrayBuffer);
        const blob = new Blob([arrayBuffer], { type: mimeType });
        console.log(blob);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    }
    catch (error) {
        console.error('Error downloading file:', error);
    }
}

// toggle password type (show/hide)
window.togglePasswordType = (elementId, isShow) => {
    const el = document.getElementById(elementId);
    if (!el) return;
    const input = el.querySelector("input");
    if (input) {
        input.type = isShow ? "text" : "password";
    }
};


// khởi tạo file để preview trước. người dùng muốn tải thì tải
async function createUrlViewer(contentStreamReference, type) {
    var url = "";
    if (type == ".pdf") {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        var file = new Blob([arrayBuffer], { type: "application/pdf" });
    }
    else if (type == ".docx") {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        var file = new Blob([arrayBuffer], { type: "application/vnd.openxmlformats-officedocument.wordprocessingml.document" });
    }
    else {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        var file = new Blob([arrayBuffer], { type: "image/png" });
    }
    url = URL.createObjectURL(file);
    return url
}

// trigger click 1 element
window.triggerClick = function (element) {
    element.click();
};

// toggle offcanvas
window.toggleOffcanvas = (elementId) => {
    const el = document.getElementById(elementId);
    if (!el) return;
    let myOffcanvas = new bootstrap.Offcanvas(el);
    myOffcanvas.show();
}