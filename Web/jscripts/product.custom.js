
/* Update price visible to user if a variant is extra */
$(".size-select").on("change", function () {
    var value = $(this).val();
    var description = $(".add-to-cart-selectors").find(".size-select > option[value='" + value + "']")[0].innerHTML;
    var modifier = 0.0;
    if (description.indexOf("[") != -1) {
        //Antique Brass [+$4.66]
        var modifier = parseFloat(description.slice(description.indexOf("[") + 1, -1).replace("$", ""));
    }

    //update visible price for user
    var actual_price = parseFloat($("meta[itemprop=price]").attr("content").replace("$", ""));
    var new_price = parseFloat(modifier + actual_price);
    $(".price-wrap").find(".variant-price").html('<span>Price:</span> $' + new_price);
});