   

$( function() {
    $("#dataDate").datepicker({
        dateFormat: 'yy-mm-dd',
        showOn: "button",
        buttonImage: "../content/images/calendar.gif",
        buttonImageOnly: true,
        buttonText: "Select date"
    });

    $("#dataDate").change(function () {
      
        DisplayExpirationDates();

    });

    $("#symbol").change(function () {

        DisplayExpirationDates();

    });

    function DisplayExpirationDates()
    {
        var vDate = $("#dataDate").val();
        var vSymbol = $('#symbol').val();


        //  alert('symbol: ' + vSymbol);
        $('#expirationDate').empty();
        $.ajax({
            url: "/freedata/getexpirationdates?symbol=" + vSymbol + "&datadate=" + vDate,
            //data: [myVar = {id: 4, email: 'emailaddress', myArray: [1, 2, 3]}];
            success: function (response) {
                //Do Something
                //alert(response);

                // Add the list of expiration dates to the drop down here
                var expDates = response;// [1, 2, 3, 4, 5];
                var option = '';
                for (var i = 0; i < expDates.length; i++) {
                    option += '<option value="' + expDates[i] + '">' + expDates[i] + '</option>';
                }
                $('#expirationDate').append(option);
            },
            error: function (xhr) {
                //Do Something to handle error
            }
        });
    }

} );

function DisplayOptionData()
{
    var vDate = $("#dataDate").val();
    var vSymbol = $('#symbol').val();
    var vExpirationDate = $("#expirationDate").val();


    jQuery.support.cors = true;
    $.ajax({
        url: "/freedata/getoptiondatajson?symbol=" + vSymbol + "&datadate=" + vDate + "&expirationDate=" + vExpirationDate,
        // url: "/OptionData/getoptiondatajson?symbol=spx&datadate=2017-10-10&expirationdate=2019-06-21",
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            WriteReponse(data);
        },
        error: function (x, y, z) {
            alert(x + '\n' + y + '\n' + z);
        }
    });
}

function WriteReponse(data)
{
    // alert(data);
    //<div class="container-fluid">
    //<div class="table-responsive">
    var strResult = "<div class='container-fluid'><div class='table-responsive'><table class='table'><tr><th>ExpirationDate</th>"
        + "<th>AskPrice</th><th>BidPrice</th><th>LastPrice</th>"
        + "<th>PutCall</th><th>StrikePrice</th><th>Volume</th><th>ImpliedVolatility</th><th>Delta</th><th>Gamma</th><th>Vega</th>"
    +"<th>OpenInterest</th><th>UnderlyingPrice</th></tr>";
    $.each(data, function (index, data) {
        //alert(data.ExpirationDate);

        strResult += "<tr><td>" + data.ExpirationDate   + "</td><td> " + data.AskPrice + "</td><td>" + data.BidPrice  + "</td><td>"

              + data.LastPrice + "</td><td>"
             + data.PutCall + "</td><td>"
              + data.StrikePrice + "</td><td>"
              + data.Volume + "</td><td>"
              + data.ImpliedVolatility + "</td><td>"
             + data.Delta + "</td><td>"
              + data.Gamma + "</td><td>"
              + data.Vega + "</td><td>"

              + data.OpenInterest + "</td><td>"
              + data.UnderlyingPrice
            + "</td></tr>";
    });
    strResult += "</table></div></div>";
    $("#divResult").html(strResult);


}
