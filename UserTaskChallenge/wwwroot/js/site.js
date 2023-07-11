var Frkn = {
    GetData: function (op, successFunc, errorFunc) {
        var options = op;
        var jqxhr;

        if (successFunc) {
            options = $.extend(options, { success: successFunc });
        }
        if (errorFunc) {
            options = $.extend(options, { error: errorFunc });
        }

        options = $.extend({}, { type: "GET", cache: false, timeout: 90000 }, options);

        if (!options.url) {
            console.log('id, name, url : undefined');
        } else {
            jqxhr = $.ajax(options);
        }
        return jqxhr;
    },
    TableOP: function (e, t) {
        var a = $.extend({}, { type: "POST", cache: false, timeout: 90000 }, { url: "/System/TableOperations", data: { jsonData: JSON.stringify(e) } });
        t && (a = $.extend(a, { success: t })), console.log(a), $.ajax(a)
    }
}

