function show_map() {
    var id = document.getElementById('b_image');
    id = id.getAttribute('src')
    var parse = id.split('/');
    id = parse[parse.length - 1];
    id = "/Upload/GetImageGPS/" + id;
    $.get(id, function (data) {
        if (data) {
            var p = data.split(" ");
            var latitude = parseFloat(p[1]) + (parseFloat(p[2]) / 60) + (parseFloat(p[3]) / 3600);
            if (p[0] == "S")
                latitude = -latitude;
            var longitude = parseFloat(p[5]) + (parseFloat(p[6]) / 60) + (parseFloat(p[7]) / 3600);
            if (p[4] == "W")
                longitude = -longitude;
            $('#map').show();
            var mapCanvas = document.getElementById('map');
            var coordinates = { lat: latitude, lng: longitude };
            var mapOptions = {
                center: coordinates,
                zoom: 8,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            }
            var map = new google.maps.Map(mapCanvas, mapOptions)

            var marker = new google.maps.Marker({
                position: coordinates,
                map: map,
                title: 'Hello World!'
            });
        }
        else $('#map').hide();
    });

}
function get_data(src) {

    get_info(src);
    get_image(src);
    get_userdescription(src);
    show_map();

}
function get_info(src) {
    var parse = src.split('/');
    var id = parse[parse.length - 1];
    id = "/Upload/GetImageInfo/" + id;
    $.get(id, function (data) {
        $("#viewinfo").html(data);
    });
    $("#exifhead").show();
}
function get_image(src) {
    var parser = document.createElement('a');
    parser.href = src;
    var url = parser.pathname;
    url = "<img src=\"" + url + "\" id=\"b_image\" class=\"img-responsive img-thumbnail /*img-hover*/ allinfo\">";
    $("#viewimage").html(url);
}
function get_userdescription(src) {
    var parse = src.split('/');
    var id = parse[parse.length - 1];
    $.post("Upload/GetSetComments", { Id: id, Text: null }, function (data) {
        $("#userdescription").html(data);
        replaceText();
    });
    $("#userdescrhead").show();
}

function removeimage() {
    var id = document.getElementById('b_image');
    id = id.getAttribute('src')
    var parse = id.split('/');
    id = parse[parse.length - 1];
    id = "/Upload/RemoveImage/" + id;
    $.get(id, function (data) {
        if (data)
            location.reload();
    });
}
