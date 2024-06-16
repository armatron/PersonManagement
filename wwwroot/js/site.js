// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// remove colned item from item list
function RemoveCloneContainer(button) {
    $(button).parent().remove();
}

// clone item and append to item list
function AddCloneContainer(template, cloneid) {

    // clone templagte content
    let clone = template.clone();
    clone[0].id = cloneid;
    clone[0].children = template[0].children;
    clone.show();

    // apend content after template
    template[0].parentNode.insertBefore(clone[0], template[0].nextSibling);
    // append content on the bottom
    //template[0].parentNode.appendChild(clone[0]);
}