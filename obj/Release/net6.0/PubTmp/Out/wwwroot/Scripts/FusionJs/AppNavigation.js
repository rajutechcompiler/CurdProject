class DataAjaxCall {
    constructor(url, type, datatype, data, contenttype, processdata, async, cache) {
        this._url = url;
        this._type = type;
        this._datatype = datatype;
        this._data = data;
        this._contenttype = contenttype;
        this._processData = processdata;
        this._async = async;
        this._cache = cache;
    }
    Send() {
        if (this.IsPropertiesPass() === false) { return; };
        return new Promise((resolve, reject) => {
            var ajaxOptions = {};
            if (this._url === "") { 0; } else { ajaxOptions.url = this._url };
            if (this._type === "") { 0; } else { ajaxOptions.type = this._type };
            if (this._datatype === "") { 0; } else { ajaxOptions.dataType = this._datatype }
            if (this._data === "") { 0; } else { ajaxOptions.data = this._data };
            if (this._async === "") { 0; } else { ajaxOptions.async = this._async };
            if (this._cache === "") { 0; } else { ajaxOptions.cache = this._cache; };
            if (this._processData === "") { 0; } else { ajaxOptions.processData = this._processData; };
            if (this._contenttype === "") { 0; } else { ajaxOptions.contentType = this._contenttype; };
            $.ajax(ajaxOptions).done(function (data) {
                resolve(data);
            }).fail(function (jqXHR) {
                showAjaxReturnMessage(jqXHR.statusText + "can't reach the server or server rejected the request, please contact your system administrator!", "e");
                reject();
                // check raju reload window
                setTimeout(() => {
                    window.location.reload();
                }, 3000);

            });
        });
    }
    IsPropertiesPass() {
        var ispass = true;
        if (this._url === undefined || this._url === "") {
            ispass = false;
            alert("Url: is a mendatory param to pass to constructor!!");
        }
        if (this._type === undefined || this._type === "") {
            ispass = false;
            alert("type: is a mendatory param to pass to constructor!!");
        }
        if (this._datatype === undefined || this._datatype === "") {
            ispass = false;
            alert("Datatype: is a mendatory param to pass to constructor!!");
        }
        if (this._data === undefined) {
            ispass = false;
            alert("Data: if you don't have anyting to pass then pass empty string to constructor!!");
        }
        if (this._contenttype === undefined) {
            ispass = false;
            alert("ContentType: if you don't have anyting to pass then pass empty string to constructor!!");
        }
        if (this._processData === undefined) {
            ispass = false;
            alert("ProcessData: if you don't have anyting to pass then pass empty string to constructor!!");
        }
        if (this._async === undefined) {
            ispass = false;
            alert("Async: if you don't have anyting to pass then pass empty string to constructor!!");
        }
        if (this._cache === undefined) {
            ispass = false;
            alert("Cache: if you don't have anyting to pass then pass empty string to constructor!!");
        }

        return ispass;
    }
}
//properties to use by DataAjaxCall
var ajax = {
    Type: {
        Post: "POST",
        Get: "GET",
        Put: "PUT",
        Delete: "DELETE"
    },
    DataType: {
        Json: "json",
        Html: "html",
        Text: "text",
        String: "string",
        TextCsv: "text/csv"
    },
    ContentType: {
        Utf8: "application/json; charset=utf-8",
        AppJson: "application/json",
        False: false,
        True: true
    },
    ProcessData: {
        False: false,
        True: true
    },
    Async: {
        False: false,
        True: true
    },
    Cache: {
        False: false,
        True: true
    }
}

//Fetch calling server data
async function FETCHGET(url, requestData) {
    var rdata = ""
    const call = await fetch(`${url}`);
    if (requestData === "html")
        rdata = await call.text();
    else if (requestData === "json")
        rdata = await call.json();
    const data = await rdata;
    console.log(data)
    return data;
}



async function FETCHPOST(url = '', data = {}) {
    const response = await fetch(url, {
        method: 'POST',
        cache: 'no-cache',
        //credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data) // body data type must match "Content-Type" header
    });
    return response.json();
}
//set cookies
function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = value + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = c_name + "=" + c_value;
}
//get cookies
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
//check variable size
function SizeOfVerb(object) {
    // initialise the list of objects and size
    var objects = [object];
    var size = 0;
    // loop over the objects
    for (var index = 0; index < objects.length; index++) {
        // determine the type of the object
        switch (typeof objects[index]) {
            // the object is a boolean
            case 'boolean': size += 4; break;
            // the object is a number
            case 'number': size += 8; break;
            // the object is a string
            case 'string': size += 2 * objects[index].length; break;
            // the object is a generic object
            case 'object':
                // if the object is not an array, add the sizes of the keys
                if (Object.prototype.toString.call(objects[index]) != '[object Array]') {
                    for (var key in objects[index]) size += 2 * key.length;
                }
                // loop over the keys
                for (var key in objects[index]) {
                    // determine whether the value has already been processed
                    var processed = false;
                    for (var search = 0; search < objects.length; search++) {
                        if (objects[search] === objects[index][key]) {
                            processed = true;
                            break;
                        }
                    }
                    // queue the value to be processed if appropriate
                    if (!processed) objects.push(objects[index][key]);
                }
        }
    }
    // return the calculated size
    if (size > 1000) {
        size = size * 0.001 + "kb";
    }
    return size;
}
//get next month
function SetNextMonthDate() {
    var tm = new Date();
    var month = (tm.getMonth() + 2).toString().length == 1 ? "0" + (tm.getMonth() + 2) : (tm.getMonth() + 2);
    var day = (tm.getDate() + 1).toString().length == 1 ? "0" + (tm.getDate() + 1) : (tm.getDate() + 1);
    var strDate = tm.getFullYear() + "-" + month + "-" + day;
    return strDate;
}
//set date
function SetCurrentDate() {
    var tm = new Date();
    var month = (tm.getMonth() + 1).toString().length == 1 ? "0" + (tm.getMonth() + 1) : (tm.getMonth() + 1);
    var day = (tm.getDate() + 1).toString().length == 1 ? "0" + (tm.getDate() + 1) : (tm.getDate() + 1);
    var strDate = tm.getFullYear() + "-" + month + "-" + day;
    return strDate;
}
//increas one month
function IncreaseDateByoneMonth(elem) {
    var increasByOneMonth = parseInt(elem.value.split("-")[1]) + 1;
    var month = increasByOneMonth.toString().length == 1 ? "0" + increasByOneMonth : increasByOneMonth;
    var year = parseInt(elem.value.split("-")[0]);
    var day = parseInt(elem.value.split("-")[2]);
    day = day.toString().length == 1 ? "0" + day : day;
    if (month == 13) {
        month = "01";
        year = year + 1;
    }
    var strDate = year + "-" + month + "-" + day;
    elem.value = strDate;
}
//get pickaday calendar
var picker = [];
function GetPickaDayCalendar(dateFormat) {
    //to call this method you have to create input element with attribute name="datepicker"
    //also you have to call function ClearPickDaysElements() before you call this funcion in order to clear the element before create a new list of dates
    var d = new Date();
    const minYear = d.getFullYear() - 200;
    const maxYear = d.getFullYear();
    $('[name="tabdatepicker"]').each(function (idx) {
        picker[idx] = new Pikaday({
            field: $(this)[0],
            format: dateFormat,
            firstDay: 0,
            yearRange: [minYear, maxYear]
        });
    });
}
//cleare calendar elemnt before you start using it
function DestroyPickDays() {
    for (var i = 0; i < picker.length; i++) {
        picker[i].destroy();
    }
    //var dt = document.querySelectorAll('[name="tabdatepicker"]')
    //var counter = 0
    //for (var i = 0; i < dt.length; i++) {
    //    dt[i].remove();
    //    counter = counter + 1;
    //}
}
//global restrictions for number type note: you can run this function everywhere in the code.
function preventCharchters() {
    //on numbers
    $('body').on("keypress", "input[type=number]", (e) => {
        if (e.keyCode == 101 || e.keyCode == 69 || e.keyCode == 45 || e.keyCode == 43) {
            return false;
        }
    })
}
//check date format
function checkDateFormat(dateValue, formatValue) {
    return moment(dateValue, formatValue).format(formatValue) === dateValue
}