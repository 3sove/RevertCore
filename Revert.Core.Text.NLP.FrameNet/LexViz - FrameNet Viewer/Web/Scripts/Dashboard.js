
function ExpandCollapseLinkButton(divMoreID, btnMoreID, hiddenFieldID, collapsedText, expandedText, displayStyle) {
    var divMore = document.getElementById(divMoreID);
    if (divMore == null) return false;

    var btnMore = document.getElementById(btnMoreID);
    if (btnMore == null) return false;

    if (displayStyle == "block") {
        divMore.style.display = "block";
        btnMore.innerText = "[ - ]";
        btnMore.innerHTML = "[ - ]";
    } else if (displayStyle == "none") {
        divMore.style.display = "none";
        btnMore.innerText = "[ + ]";
        btnMore.innerHTML = "[ + ]";
    }
    else if (divMore.style.display == "none") {
        divMore.style.display = "block";
        btnMore.innerText = "[ - ]";
        btnMore.innerHTML = "[ - ]";
    }
    else if (divMore.style.display == "block") {
        divMore.style.display = "none";
        btnMore.innerText = "[ + ]";
        btnMore.innerHTML = "[ + ]";
    }

    document.getElementById(hiddenFieldID).value = divMore.style.display;
    return false;
}

function PostAsync(requestUrl, targetDiv) {
    if (window.XMLHttpRequest) this.xmlHttpRequest = new XMLHttpRequest();
    else if (window.ActiveXObject) this.xmlHttpRequest = new ActiveXObject("Microsoft.XMLHTTP");

    this.xmlHttpRequest.open('POST', requestUrl, true);
    this.xmlHttpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    this.xmlHttpRequest.onreadystatechange = function () {
        if (this.readyState == 4) {
            document.getElementById(targetDiv).innerHTML = this.responseText;
        }
    }
    this.xmlHttpRequest.send();
}

function ToggleMore(divMoreID, btnMoreID, moreCount) {
    var divMore = document.getElementById(divMoreID);
    if (divMore == null) return false;

    var btnMore = document.getElementById(btnMoreID);
    if (btnMore == null) return false;

    if (divMore.style.display == "none") {
        divMore.style.display = "block";
        btnMore.value = "Hide " + moreCount + " Items";
    }
    else if (divMore.style.display == "block") {
        divMore.style.display = "none";
        btnMore.value = "Show " + moreCount + " More Items";
    }

    return false;
}


function ToggleEnhancedDropDownListItems(divItemsID) {
    var divItems = document.getElementById(divItemsID);
    if (divItems == null) return false;

    if (divItems.style.display == "none") {
        divItems.style.display = "block";
    }
    else if (divItems.style.display == "block") {
        divItems.style.display = "none";
    }

    return false;
}

