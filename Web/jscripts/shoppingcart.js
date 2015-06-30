  if (document.getElementById("Quantity_%SKU%").value == "")
  {
    alert("Please enter a quantity in the \"Quantity\" field. To remove an item from the cart, set its quantity to 0.");
    document.getElementById("Quantity_%SKU%").focus();
    submitenabled(theForm);
    return (false);
  }

  if (document.getElementById("Quantity_%SKU%").value < 0)
  {
    alert("Only numbers 0 or higher are allowed in the \"Quantity\" field. To remove an item from the cart, set its quantity to 0.");
    document.getElementById("Quantity_%SKU%").value = 1;
    document.getElementById("Quantity_%SKU%").focus();
    submitenabled(theForm);
    return (false);
  }

  if (document.getElementById("Quantity_%SKU%").value.length > 4)
  {
    alert("The maximum allowed quantity is 9999.");
    document.getElementById("Quantity_%SKU%").focus();
    submitenabled(theForm);
    return (false);
  }

  var checkOK = "0123456789";
  var checkStr = document.getElementById("Quantity_%SKU%").value;
  var allValid = true;
  var decPoints = 0;
  var allNum = "";
  for (i = 0;  i < checkStr.length;  i++)
  {
    ch = checkStr.charAt(i);
    for (j = 0;  j < checkOK.length;  j++)
      if (ch == checkOK.charAt(j))
        break;
    if (j == checkOK.length)
    {
      allValid = false;
      break;
    }
    allNum += ch;
  }
  if (!allValid)
  {
    alert("Please enter only numbers in the \"Quantity\" field.");
    document.getElementById("Quantity_%SKU%").focus();
    submitenabled(theForm);
    return (false);
  }
  submitenabled(theForm);
	