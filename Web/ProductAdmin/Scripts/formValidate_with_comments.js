/*
   Script:                 formValidate.js

   Source:                 http://www.shastrynet.com/

   Author:                 Subramanya Shastry Y V H
						   shastry@email.com

   Description:            Generic form validation routine.

   Copyright:              This script can be freely used with or without modifications.
  
                           The author cannot be held responsible for any possible mistakes
                           in the coding. It is the responsibility of the user to make
                           sure that the code is working fine and is bug-less.
  
                           This Script cannot be distributed without prior permission from
                           the author.
*/



function Trim(TRIM_VALUE){
if(TRIM_VALUE.length < 1){
return"";
}
TRIM_VALUE = RTrim(TRIM_VALUE);
TRIM_VALUE = LTrim(TRIM_VALUE);
if(TRIM_VALUE==""){
return "";
}
else{
return TRIM_VALUE;
}
} //End Function

function RTrim(VALUE){
var w_space = String.fromCharCode(32);
var v_length = VALUE.length;
var strTemp = "";
if(v_length < 0){
return"";
}
var iTemp = v_length -1;

while(iTemp > -1){
if(VALUE.charAt(iTemp) == w_space){
}
else{
strTemp = VALUE.substring(0,iTemp +1);
break;
}
iTemp = iTemp-1;

} //End While
return strTemp;

} //End Function

function LTrim(VALUE){
var w_space = String.fromCharCode(32);
if(v_length < 1){
return"";
}
var v_length = VALUE.length;
var strTemp = "";

var iTemp = 0;

while(iTemp < v_length){
if(VALUE.charAt(iTemp) == w_space){
}
else{
strTemp = VALUE.substring(iTemp,v_length);
break;
}
iTemp = iTemp + 1;
} //End While
return strTemp;
} //End Function


// isWhitespace (s)                    Check whether string s is empty or whitespace.
// isLetter (c)                        Check whether character c is an English letter 
// isDigit (c)                         Check whether character c is a digit 
// isLetterOrDigit (c)                 Check whether character c is a letter or digit.
// isInteger (s [,eok])                True if all characters in string s are numbers.
// isSignedInteger (s [,eok])          True if all characters in string s are numbers; leading + or - allowed.
// isPositiveInteger (s [,eok])        True if string s is an integer > 0.
// isNonnegativeInteger (s [,eok])     True if string s is an integer >= 0.
// isNegativeInteger (s [,eok])        True if s is an integer < 0.
// isNonpositiveInteger (s [,eok])     True if s is an integer <= 0.
// isFloat (s [,eok])                  True if string s is an unsigned floating point (real) number. (Integers also OK.)
// isSignedFloat (s [,eok])            True if string s is a floating point number; leading + or - allowed. (Integers also OK.)
// isAlphabetic (s [,eok])             True if string s is English letters 
// isAlphanumeric (s [,eok])           True if string s is English letters and numbers only.

// FUNCTIONS TO REFORMAT DATA:
//
// stripCharsInBag (s, bag)            Removes all characters in string bag from string s.
// stripCharsNotInBag (s, bag)         Removes all characters NOT in string bag from string s.
// stripWhitespace (s)                 Removes all whitespace characters from s.
// stripInitialWhitespace (s)          Removes initial (leading) whitespace characters from s.
// reformat (TARGETSTRING, STRING,     Function for inserting formatting characters or
//   INTEGER, STRING, INTEGER ... )       delimiters into TARGETSTRING.                                       
// reformatZIPCode (ZIPString)         If 9 digits, inserts separator hyphen.
// reformatSSN (SSN)                   Reformats as 123-45-6789.
// reformatUSPhone (USPhone)           Reformats as (123) 456-789.


// VARIABLE DECLARATIONS

var digits = "0123456789";
var lowercaseLetters = "abcdefghijklmnopqrstuvwxyz"
var uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
var whitespace = " \t\n\r";

// decimal point character differs by language and culture
var decimalPointDelimiter = "."

// non-digit characters which are allowed in phone numbers
var phoneNumberDelimiters = "()- ";

// characters which are allowed in US phone numbers
var validUSPhoneChars = digits + phoneNumberDelimiters;

// characters which are allowed in international phone numbers
// (a leading + is OK)
var validWorldPhoneChars = digits + phoneNumberDelimiters + "+";

// non-digit characters which are allowed in 
// Social Security Numbers
var SSNDelimiters = "- ";

// characters which are allowed in Social Security Numbers
var validSSNChars = digits + SSNDelimiters;

// U.S. Social Security Numbers have 9 digits.
// They are formatted as 123-45-6789.
var digitsInSocialSecurityNumber = 9;

// U.S. phone numbers have 10 digits.
// They are formatted as 123 456 7890 or (123) 456-7890.
var digitsInUSPhoneNumber = 10;

// non-digit characters which are allowed in ZIP Codes
var ZIPCodeDelimiters = "-";

// our preferred delimiter for reformatting ZIP Codes
var ZIPCodeDelimeter = "-"

// characters which are allowed in Social Security Numbers
var validZIPCodeChars = digits + ZIPCodeDelimiters

// U.S. ZIP codes have 5 or 9 digits.
// They are formatted as 12345 or 12345-6789.
var digitsInZIPCode1 = 5
var digitsInZIPCode2 = 9

// non-digit characters which are allowed in credit card numbers
var creditCardDelimiters = " "

// returns true if s only contains characters that are in bag
function isOkBag (s, bag)

{   var i;
    var returnString = "";

    // Search through string's characters one by one.
    // If character is not in bag, return false.
    for (i = 0; i < s.length; i++)
    {   
        var c = s.charAt(i);
        if (bag.indexOf(c) == -1) return false;
    }
    return true;
}

// Check whether string s is empty.
function isEmpty(s)
{   return ((s == null) || (s.length == 0))
}

// Returns true if string s is empty or 
// whitespace characters only.
function isWhiteSpace (s)
{   var i;

    // Is s empty?
    if (isEmpty(s)) return true;

    // Search through string's characters one by one
    // until we find a non-whitespace character.
    // When we do, return false; if we don't, return true.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character isn't whitespace.
        var c = s.charAt(i);

        if (whitespace.indexOf(c) == -1) return false;
    }

    // All characters are whitespace.
    return true;
}

// Removes all characters which appear in string bag from string s.
function stripCharsInBag (s, bag)

{   var i;
    var returnString = "";

    // Search through string's characters one by one.
    // If character is not in bag, append to returnString.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character isn't whitespace.
        var c = s.charAt(i);
        if (bag.indexOf(c) == -1) returnString += c;
    }

    return returnString;
}

// Removes all characters which do NOT appear in string bag 
// from string s.
function stripCharsNotInBag (s, bag)

{   var i;
    var returnString = "";

    // Search through string's characters one by one.
    // If character is in bag, append to returnString.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character isn't whitespace.
        var c = s.charAt(i);
        if (bag.indexOf(c) != -1) returnString += c;
    }

    return returnString;
}

// Removes all whitespace characters from s.
// Global variable whitespace (see above)
// defines which characters are considered whitespace.
function stripWhitespace (s)

{   return stripCharsInBag (s, whitespace)
}

// WORKAROUND FUNCTION FOR NAVIGATOR 2.0.2 COMPATIBILITY.
//
// The below function *should* be unnecessary.  In general,
// avoid using it.  Use the standard method indexOf instead.
//
// However, because of an apparent bug in indexOf on 
// Navigator 2.0.2, the below loop does not work as the
// body of stripInitialWhitespace:
//
// while ((i < s.length) && (whitespace.indexOf(s.charAt(i)) != -1))
//   i++;
//
// ... so we provide this workaround function charInString
// instead.
//
// charInString (CHARACTER c, STRING s)
//
// Returns true if single character c (actually a string)
// is contained within string s.

function charInString (c, s)
{   for (i = 0; i < s.length; i++)
    {   if (s.charAt(i) == c) return true;
    }
    return false
}

// Removes initial (leading) whitespace characters from s.
// Global variable whitespace (see above)
// defines which characters are considered whitespace.
function stripInitialWhitespace (s)

{   var i = 0;

    while ((i < s.length) && charInString (s.charAt(i), whitespace))
       i++;
    
    return s.substring (i, s.length);
}

// Returns true if character c is an English letter (A .. Z, a..z).
//
// NOTE: Need i18n version to support European characters.
// This could be tricky due to different character
// sets and orderings for various languages and platforms.
function isLetter (c)
{   return ( ((c >= "a") && (c <= "z")) || ((c >= "A") && (c <= "Z")) )
}

// Returns true if character c is a digit (0 .. 9).
function isDigit (c)
{   return ((c >= "0") && (c <= "9"))
}

// Returns true if character c is a letter or digit.
function isLetterOrDigit (c)
{   return (isLetter(c) || isDigit(c))
}


// isInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if all characters in string s are numbers.
//
// Accepts non-signed integers only. Does not accept floating 
// point, exponential notation, etc.
//
// We don't use parseInt because that would accept a string
// with trailing non-numeric characters.
//
// By default, returns defaultEmptyOK if s is empty.
// There is an optional second argument called emptyOK.
// emptyOK is used to override for a single function call
//      the default behavior which is specified globally by
//      defaultEmptyOK.
// If emptyOK is false (or any value other than true), 
//      the function will return false if s is empty.
// If emptyOK is true, the function will return true if s is empty.
//
// EXAMPLE FUNCTION CALL:     RESULT:
// isInteger ("5")            true 
// isInteger ("")             defaultEmptyOK
// isInteger ("-5")           false
// isInteger ("", true)       true
// isInteger ("", false)      false
// isInteger ("5", false)     true

function isInteger (s)

{   var i;

    if (isEmpty(s)) 
       if (isInteger.arguments.length == 1) return defaultEmptyOK;
       else return (isInteger.arguments[1] == true);

    // Search through string's characters one by one
    // until we find a non-numeric character.
    // When we do, return false; if we don't, return true.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character is number.
        var c = s.charAt(i);

        if (!isDigit(c)) return false;
    }

    // All characters are numbers.
    return true;
}

// isSignedInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if all characters are numbers; 
// first character is allowed to be + or - as well.
//
// Does not accept floating point, exponential notation, etc.
//
// We don't use parseInt because that would accept a string
// with trailing non-numeric characters.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.
//
// EXAMPLE FUNCTION CALL:          RESULT:
// isSignedInteger ("5")           true 
// isSignedInteger ("")            defaultEmptyOK
// isSignedInteger ("-5")          true
// isSignedInteger ("+5")          true
// isSignedInteger ("", false)     false
// isSignedInteger ("", true)      true

function isSignedInteger (s)

{   if (isEmpty(s)) 
       if (isSignedInteger.arguments.length == 1) return defaultEmptyOK;
       else return (isSignedInteger.arguments[1] == true);

    else {
        var startPos = 0;
        var secondArg = defaultEmptyOK;

        if (isSignedInteger.arguments.length > 1)
            secondArg = isSignedInteger.arguments[1];

        // skip leading + or -
        if ( (s.charAt(0) == "-") || (s.charAt(0) == "+") )
           startPos = 1;    
        return (isInteger(s.substring(startPos, s.length), secondArg))
    }
}

// isPositiveInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is an integer > 0.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isPositiveInteger (s)
{   var secondArg = defaultEmptyOK;

    if (isPositiveInteger.arguments.length > 1)
        secondArg = isPositiveInteger.arguments[1];

    // The next line is a bit byzantine.  What it means is:
    // a) s must be a signed integer, AND
    // b) one of the following must be true:
    //    i)  s is empty and we are supposed to return true for
    //        empty strings
    //    ii) this is a positive, not negative, number

    return (isSignedInteger(s, secondArg)
         && ( (isEmpty(s) && secondArg)  || (parseInt (s) > 0) ) );
}

// isNonnegativeInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is an integer >= 0.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isNonnegativeInteger (s)
{   var secondArg = defaultEmptyOK;

    if (isNonnegativeInteger.arguments.length > 1)
        secondArg = isNonnegativeInteger.arguments[1];

    // The next line is a bit byzantine.  What it means is:
    // a) s must be a signed integer, AND
    // b) one of the following must be true:
    //    i)  s is empty and we are supposed to return true for
    //        empty strings
    //    ii) this is a number >= 0

    return (isSignedInteger(s, secondArg)
         && ( (isEmpty(s) && secondArg)  || (parseInt (s) >= 0) ) );
}


// isNegativeInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is an integer < 0.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isNegativeInteger (s)
{   var secondArg = defaultEmptyOK;

    if (isNegativeInteger.arguments.length > 1)
        secondArg = isNegativeInteger.arguments[1];

    // The next line is a bit byzantine.  What it means is:
    // a) s must be a signed integer, AND
    // b) one of the following must be true:
    //    i)  s is empty and we are supposed to return true for
    //        empty strings
    //    ii) this is a negative, not positive, number

    return (isSignedInteger(s, secondArg)
         && ( (isEmpty(s) && secondArg)  || (parseInt (s) < 0) ) );
}


// isNonpositiveInteger (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is an integer <= 0.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isNonpositiveInteger (s)
{   var secondArg = defaultEmptyOK;

    if (isNonpositiveInteger.arguments.length > 1)
        secondArg = isNonpositiveInteger.arguments[1];

    // The next line is a bit byzantine.  What it means is:
    // a) s must be a signed integer, AND
    // b) one of the following must be true:
    //    i)  s is empty and we are supposed to return true for
    //        empty strings
    //    ii) this is a number <= 0

    return (isSignedInteger(s, secondArg)
         && ( (isEmpty(s) && secondArg)  || (parseInt (s) <= 0) ) );
}


// isFloat (STRING s [, BOOLEAN emptyOK])
// 
// True if string s is an unsigned floating point (real) number. 
//
// Also returns true for unsigned integers. If you wish
// to distinguish between integers and floating point numbers,
// first call isInteger, then call isFloat.
//
// Does not accept exponential notation.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isFloat (s)

{   var i;
    var seenDecimalPoint = false;

    if (isEmpty(s)) 
       if (isFloat.arguments.length == 1) return defaultEmptyOK;
       else return (isFloat.arguments[1] == true);

    if (s == decimalPointDelimiter) return false;

    // Search through string's characters one by one
    // until we find a non-numeric character.
    // When we do, return false; if we don't, return true.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character is number.
        var c = s.charAt(i);

        if ((c == decimalPointDelimiter) && !seenDecimalPoint) seenDecimalPoint = true;
        else if (!isDigit(c)) return false;
    }

    // All characters are numbers.
    return true;
}


// isSignedFloat (STRING s [, BOOLEAN emptyOK])
// 
// True if string s is a signed or unsigned floating point 
// (real) number. First character is allowed to be + or -.
//
// Also returns true for unsigned integers. If you wish
// to distinguish between integers and floating point numbers,
// first call isSignedInteger, then call isSignedFloat.
//
// Does not accept exponential notation.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.

function isSignedFloat (s)

{   if (isEmpty(s)) 
       if (isSignedFloat.arguments.length == 1) return defaultEmptyOK;
       else return (isSignedFloat.arguments[1] == true);

    else {
        var startPos = 0;
        var secondArg = defaultEmptyOK;

        if (isSignedFloat.arguments.length > 1)
            secondArg = isSignedFloat.arguments[1];

        // skip leading + or -
        if ( (s.charAt(0) == "-") || (s.charAt(0) == "+") )
           startPos = 1;    
        return (isFloat(s.substring(startPos, s.length), secondArg))
    }
}




// isAlphabetic (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is English letters 
// (A .. Z, a..z) only.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.
//
// NOTE: Need i18n version to support European characters.
// This could be tricky due to different character
// sets and orderings for various languages and platforms.

function isAlphabetic (s)

{   var i;

    if (isEmpty(s)) 
       if (isAlphabetic.arguments.length == 1) return defaultEmptyOK;
       else return (isAlphabetic.arguments[1] == true);

    // Search through string's characters one by one
    // until we find a non-alphabetic character.
    // When we do, return false; if we don't, return true.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character is letter.
        var c = s.charAt(i);

        if (!isLetter(c))
        return false;
    }

    // All characters are letters.
    return true;
}




// isAlphanumeric (STRING s [, BOOLEAN emptyOK])
// 
// Returns true if string s is English letters 
// (A .. Z, a..z) and numbers only.
//
// For explanation of optional argument emptyOK,
// see comments of function isInteger.
//
// NOTE: Need i18n version to support European characters.
// This could be tricky due to different character
// sets and orderings for various languages and platforms.

function isAlphanumeric (s)

{   var i;

    if (isEmpty(s)) 
       if (isAlphanumeric.arguments.length == 1) return defaultEmptyOK;
       else return (isAlphanumeric.arguments[1] == true);

    // Search through string's characters one by one
    // until we find a non-alphanumeric character.
    // When we do, return false; if we don't, return true.

    for (i = 0; i < s.length; i++)
    {   
        // Check that current character is number or letter.
        var c = s.charAt(i);

        if (! (isLetter(c) || isDigit(c) ) )
        return false;
    }

    // All characters are numbers or letters.
    return true;
}




// reformat (TARGETSTRING, STRING, INTEGER, STRING, INTEGER ... )       
//
// Handy function for arbitrarily inserting formatting characters
// or delimiters of various kinds within TARGETSTRING.
//
// reformat takes one named argument, a string s, and any number
// of other arguments.  The other arguments must be integers or
// strings.  These other arguments specify how string s is to be
// reformatted and how and where other strings are to be inserted
// into it.
//
// reformat processes the other arguments in order one by one.
// * If the argument is an integer, reformat appends that number 
//   of sequential characters from s to the resultString.
// * If the argument is a string, reformat appends the string
//   to the resultString.
//
// NOTE: The first argument after TARGETSTRING must be a string.
// (It can be empty.)  The second argument must be an integer.
// Thereafter, integers and strings must alternate.  This is to
// provide backward compatibility to Navigator 2.0.2 JavaScript
// by avoiding use of the typeof operator.
//
// It is the caller's responsibility to make sure that we do not
// try to copy more characters from s than s.length.
//
// EXAMPLES:
//
// * To reformat a 10-digit U.S. phone number from "1234567890"
//   to "(123) 456-7890" make this function call:
//   reformat("1234567890", "(", 3, ") ", 3, "-", 4)
//
// * To reformat a 9-digit U.S. Social Security number from
//   "123456789" to "123-45-6789" make this function call:
//   reformat("123456789", "", 3, "-", 2, "-", 4)
//
// HINT:
//
// If you have a string which is already delimited in one way
// (example: a phone number delimited with spaces as "123 456 7890")
// and you want to delimit it in another way using function reformat,
// call function stripCharsNotInBag to remove the unwanted 
// characters, THEN call function reformat to delimit as desired.
//
// EXAMPLE:
//
// reformat (stripCharsNotInBag ("123 456 7890", digits),
//           "(", 3, ") ", 3, "-", 4)
function reformat (s)

{   var arg;
    var sPos = 0;
    var resultString = "";

    for (var i = 1; i < reformat.arguments.length; i++) {
       arg = reformat.arguments[i];
       if (i % 2 == 1) resultString += arg;
       else {
           resultString += s.substring(sPos, sPos + arg);
           sPos += arg;
       }
    }
    return resultString;
}

// takes ZIPString, a string of 5 or 9 digits;
// if 9 digits, inserts separator hyphen
function reformatZIPCode (ZIPString)
{   if (ZIPString.length == 5) return ZIPString;
    else return (reformat (ZIPString, "", 5, "-", 4));
}

// takes USPhone, a string of 10 digits
// and reformats as (123) 456-789
function reformatUSPhone (USPhone)
{   return (reformat (USPhone, "(", 3, ") ", 3, "-", 4))
}

// takes SSN, a string of 9 digits
// and reformats as 123-45-6789
function reformatSSN (SSN)
{   return (reformat (SSN, "", 3, "-", 2, "-", 4))
}

//------------------------------------------------------------------------------------
// function: isLeapYear
//           Function to tell, whether the given year is leap year or not
//------------------------------------------------------------------------------------
function isLeapYear(argYear) {
	return ((argYear % 4 == 0) && (argYear % 100 != 0)) || (argYear % 400 == 0) 
}

//------------------------------------------------------------------------------------
// function: daysInMonth
//           Function to return the maximum number of days in a given month of a
//           given year
//------------------------------------------------------------------------------------
function daysInMonth(argMonth, argYear) {
	switch (Number(argMonth)) {
		case 1:		// Jan
		case 3:		// Mar
		case 5:		// May
		case 7:		// Jul
		case 8:		// Aug
		case 10:		// Oct
		case 12:		// Dec
			return 31;
			break;
		
		case 4:		// Apr
		case 6:		// Jun
		case 9:		// Sep
		case 11:		// Nov
			return 30;
			break;
		
		case 2:		// Feb
			if (isLeapYear(argYear))
				return 29
			else
				return 28
			break;
		
		default:
			return 0;
	}
}

//------------------------------------------------------------------------------------
// function: getDateSeparator
//           Function to return the date separator
//           This function expects date in the format of mm/dd/yyyy or mm/dd/yy
//           or mm-dd-yyyy or mm-dd-yy
//------------------------------------------------------------------------------------
function getDateSeparator(argDate) {
	// Are there invalid separators?
	if ((argDate.indexOf('-') > 0) && (argDate.indexOf('/') > 0))
		return ' '

	if (argDate.indexOf('-') > 0)
		return '-'
	else
		if (argDate.indexOf('/') > 0)
			return '/'
		else
			return ' '
}

//------------------------------------------------------------------------------------
// function: getYear
//           Function to return the year part of the given date.
//           This function expects date in the format of mm/dd/yyyy or mm/dd/yy
//           or mm-dd-yyyy or mm-dd-yy
//------------------------------------------------------------------------------------
function getYear(argDate) {
	var dateSep = getDateSeparator(argDate)
	
	if (dateSep == ' ')
		return 0

	if(argDate.split(dateSep).length == 3)
		return argDate.split(dateSep)[2]
	else
		return 0
}

//------------------------------------------------------------------------------------
// function: getMonth
//           Function to return the month part of the given date.
//           This function expects date in the format of mm/dd/yyyy or mm/dd/yy
//           or mm-dd-yyyy or mm-dd-yy
//------------------------------------------------------------------------------------
function getMonth(argDate) {
	var dateSep = getDateSeparator(argDate)
	
	if (dateSep == ' ')
		return 0

	if(argDate.split(dateSep).length == 3)
		return argDate.split(dateSep)[0]
	else
		return 0
}

//------------------------------------------------------------------------------------
// function: getDay
//           Function to return the day part of the given date.
//           This function expects date in the format of mm/dd/yyyy or mm/dd/yy
//           or mm-dd-yyyy or mm-dd-yy
//------------------------------------------------------------------------------------
function getDay(argDate) {
	var dateSep = getDateSeparator(argDate)
	
	if (dateSep == ' ')
		return 0

	if(argDate.split(dateSep).length == 3)
		return argDate.split(dateSep)[1]
	else
		return 0
}

//------------------------------------------------------------------------------------
// function: isProperDay
//           Function to tell whether the given day of the given month is valid
//------------------------------------------------------------------------------------
function isProperDay(argDay, argMonth, argYear) {
	if ((isWhiteSpace(argDay)) || (argDay == 0))
		return false

	if ((argDay > 0) && (argDay < daysInMonth(argMonth, argYear) + 1))
		return true
	else 
		return false
}

//------------------------------------------------------------------------------------
// function: isProperMonth
//           Function to tell whether the given month is a valid one
//------------------------------------------------------------------------------------
function isProperMonth(argMonth) {
	if ((isWhiteSpace(argMonth)) || (argMonth == 0))
		return false
	
	if ((argMonth > 0) && (argMonth < 13))
		return true
	else
		return false
}

//------------------------------------------------------------------------------------
// function: isProperYear
//           Function to tell whether the given Year is a valid one
//------------------------------------------------------------------------------------
function isProperYear(argYear) {
	if ((isWhiteSpace(argYear)) || (argYear.toString().length > 4) || (argYear.toString().length == 3))
		return false
	
	switch (argYear.toString().length) {
		case 1:
			if (argYear >=0 && argYear < 10)
				return true
			else
				return false
			
		case 2:
			if (argYear >=0 && argYear < 100)
				return true
			else
				return false
			
		case 4:
			if (((argYear >=1900) || (argYear >=2000)) && ((argYear < 3000) || (argYear < 2000)))
				return true
			else
				return false
		
		default:
			return false
	}
}

//------------------------------------------------------------------------------------
// function: isProperDate
//           Function to tell whether the given date is valid or not
//           This function expects date in the format of mm/dd/yyyy or mm/dd/yy
//           or mm-dd-yyyy or mm-dd-yy
//------------------------------------------------------------------------------------
function isProperDate(argDate) {
	var tmpDay = getDay(argDate)
	var tmpMon = getMonth(argDate)
	var tmpYear = getYear(argDate)

	return isProperDay(tmpDay, tmpMon, tmpYear) && isProperMonth(tmpMon) && isProperYear(tmpYear)
}

//------------------------------------------------------------------------------------
// function: charOccurrences
//           Function to return the number of times a given character is occuring in
//           the given string
//------------------------------------------------------------------------------------
function charOccurrences(argString, argChar) {
	var intCt = 0

	for(var intI=0; intI < argString.length; intI++)
		if (argString.charAt(intI) == argChar)
			intCt++
	
	return intCt
}

//------------------------------------------------------------------------------------
// function: isProperEmail
//           Function to tell whether the given email is valid or not
//------------------------------------------------------------------------------------
function isProperEmail(argEmail) {
	if (charOccurrences(argEmail, '@') + charOccurrences(argEmail, '.') < 2)
		return false

	var atPos = argEmail.indexOf('@')
	var dotPos = argEmail.indexOf('.')

	if((atPos == 0) || (atPos == (argEmail.length - 1)))
		return false

	if((dotPos == 0) || (dotPos == (argEmail.length - 1)))
		return false
	
	/* This logic is incorrect JMA 12/18/03 */
	/* if((atPos > dotPos) || (atPos == (dotPos - 1)))
		return false */
	
	/* The following variable tells the rest of the function whether or not
	to verify that the address ends in a two-letter country or well-known
	TLD.  1 means check it, 0 means don't. */
 
	var checkTLD=1;
 
	/* The following is the list of known TLDs that an e-mail address must end with. */
 
	var knownDomsPat=/^(com|net|org|edu|int|mil|gov|arpa|biz|aero|name|coop|info|pro|museum)$/;
 
	/* The following pattern is used to check if the entered e-mail address
	fits the user@domain format.  It also is used to separate the username
	from the domain. */
 
	var emailPat=/^(.+)@(.+)$/;
 
	/* The following string represents the pattern for matching all special
	characters.  We don't want to allow special characters in the address. 
	These characters include ( ) < > @ , ; : \ " . [ ] */
 
	var specialChars="\\(\\)><@,;:\\\\\\\"\\.\\[\\]";
 
	/* The following string represents the range of characters allowed in a 
	username or domainname.  It really states which chars aren't allowed.*/
 
	var validChars="\[^\\s" + specialChars + "\]";
 
	/* The following pattern applies if the "user" is a quoted string (in
	which case, there are no rules about which characters are allowed
	and which aren't; anything goes).  E.g. "jiminy cricket"@disney.com
	is a legal e-mail address. */
 
	var quotedUser="(\"[^\"]*\")";
 
	/* The following pattern applies for domains that are IP addresses,
	rather than symbolic names.  E.g. joe@[123.124.233.4] is a legal
	e-mail address. NOTE: The square brackets are required. */
 
	var ipDomainPat=/^\[(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})\]$/;
 
	/* The following string represents an atom (basically a series of non-special characters.) */
 
	var atom=validChars + '+';
 
	/* The following string represents one word in the typical username.
	For example, in john.doe@somewhere.com, john and doe are words.
	Basically, a word is either an atom or quoted string. */
 
	var word="(" + atom + "|" + quotedUser + ")";
 
	// The following pattern describes the structure of the user
 
	var userPat=new RegExp("^" + word + "(\\." + word + ")*$");
 
	/* The following pattern describes the structure of a normal symbolic
	domain, as opposed to ipDomainPat, shown above. */
 
	var domainPat=new RegExp("^" + atom + "(\\." + atom +")*$");
 
	/* Finally, let's start trying to figure out if the supplied address is valid. */
 
	/* Begin with the coarse pattern to simply break up user@domain into
	different pieces that are easy to analyze. */
 
	var matchArray=argEmail.match(emailPat);
 
	if (matchArray==null)
		{
 
		/* Too many/few @'s or something; basically, this address doesn't
		even fit the general mould of a valid e-mail address. */
 
		return false;
		}
	var user=matchArray[1];
	var domain=matchArray[2];
 
	// Start by checking that only basic ASCII characters are in the strings (0-127).
 
	for (i=0; i<user.length; i++)
		{
		if (user.charCodeAt(i)>127)
			{
			return false;
			}
		}
	for (i=0; i<domain.length; i++)
		{
		if (domain.charCodeAt(i)>127)
			{
			return false;
			}
		}
 
	// See if "user" is valid 
 
	if (user.match(userPat)==null)
		{
		// user is not valid
		return false;
	}
 
	/* if the e-mail address is at an IP address (as opposed to a symbolic
	host name) make sure the IP address is valid. */
 
	var IPArray=domain.match(ipDomainPat);
	if (IPArray!=null)
		{
		// this is an IP address
		for (var i=1;i<=4;i++)
			{
			if (IPArray[i]>255)
				{
				return false;
				}
			}
		return true;
		}
 
	// Domain is symbolic name.  Check if it's valid.
 
	var atomPat=new RegExp("^" + atom + "$");
	var domArr=domain.split(".");
	var len=domArr.length;
	for (i=0;i<len;i++)
		{
		if (domArr[i].search(atomPat)==-1)
			{
			return false;
			}
		}
 
	/* domain name seems valid, but now make sure that it ends in a
	known top-level domain (like com, edu, gov) or a two-letter word,
	representing country (uk, nl), and that there's a hostname preceding 
	the domain or country. */
 
	if (checkTLD && domArr[domArr.length-1].length!=2 && domArr[domArr.length-1].search(knownDomsPat)==-1)
		{
		return false;
		}
 
	// Make sure there's a host name preceding the domain.
	if (len<2)
		{
		return false;
		}
 
	// If we've gotten this far, everything's valid!
	return true;
}

//------------------------------------------------------------------------------------
// function: isProperNumber
//           Function to tell whether the given string is a proper number
//------------------------------------------------------------------------------------
function isProperNumber(argNumber) {
	var numberValue = Number(argNumber)
	
	if (isNaN(numberValue)) 
		return false
	else
		return !isWhiteSpace(argNumber)
}

//------------------------------------------------------------------------------------
// function: isProperAlphabetic
//           Function to tell whether the given string is a proper alphabetic string
//------------------------------------------------------------------------------------
function isProperAlphabetic(argString) {
	var alphabets = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ"

	for(var intI=0; intI<argString.length; intI++)
		if (alphabets.indexOf(argString.charAt(intI)) == -1)
			return false
	
	return true
}

//------------------------------------------------------------------------------------
// function: objectValue
//           Function to return the value of the form element
//------------------------------------------------------------------------------------
function objectValue(argFrm, argElem) {
	var intI
	var objElem = null

	for (intI=0; intI<argFrm.length; intI++)
		if (argFrm[intI].name == argElem) 
			objElem = argFrm[intI]

	switch (objElem.type) {
		case 'text':
		case 'hidden':
		case 'password':
			return objElem.value
			break;
		
		case 'select-one':
			if (objElem.length == 0)
				return ''
			else 
				return objElem.options[objElem.selectedIndex].value
			break;
		
		case 'radio':
			for (intI=0; intI<argFrm.length; intI++)
				if (argFrm[intI].name == argElem) 
					if (argFrm[intI].checked)
						return argFrm[intI].value

			return ''
			break;
	}
}

//------------------------------------------------------------------------------------
// function: objectFocus
//           Function to set the focus to the form element
//------------------------------------------------------------------------------------
function objectFocus(argFrm, argElem) {
	var intI
	var objElem = null
	for (intI=0; intI<argFrm.length; intI++)
		if (argFrm[intI].name == argElem) 
			objElem = argFrm[intI]
	objElem.focus();
}

//------------------------------------------------------------------------------------
// function: isProperZip
//           Function to validate a Zip code
//------------------------------------------------------------------------------------
function isProperZip(argZip) {
	if ((argZip.length == 5) || (argZip.length == 9))
		return isProperNumber(argZip)
	
	if (argZip.length == 10)
		return (isProperNumber(argZip.substr(0, 5)) && isProperNumber(argZip.substr(6, 4)) & (argZip.charAt(5) == '-'))
}

//------------------------------------------------------------------------------------
// returns true if string s is a valid U.S. Phone Number.  Must be 10 digits.
//------------------------------------------------------------------------------------
function isProperUSPhone (argPhone)
{
	var argPhone2 = stripCharsNotInBag(argPhone,"0123456789")
    return (isOkBag(argPhone,"01234567890 -().") && isInteger(argPhone2) && argPhone2.length==digitsInUSPhoneNumber)
}

//------------------------------------------------------------------------------------
// function: isProperUSSSN
//------------------------------------------------------------------------------------
function isProperUSSSN(argSSN) {
	var argSSN2 = stripCharsNotInBag(argSSN,"0123456789")
    return (isOkBag(argSSN,"01234567890-") && isInteger(argSSN2) && argSSN2.length==11)
}

//------------------------------------------------------------------------------------
// function: actionFields
//           Function to parse the action arguments
//------------------------------------------------------------------------------------
function actionFields(argActions) {
	this.email			= (argActions.indexOf('[email]') > -1)
	this.required		= (argActions.indexOf('[req]') > -1)
	this.checkDate		= (argActions.indexOf('[date]') > -1)
	this.checkZip		= (argActions.indexOf('[zip]') > -1)
	this.checkNumber	= (argActions.indexOf('[number]') > -1)
	this.checkAlphabetic= (argActions.indexOf('[alpha]') > -1)
	this.checkUSPhone	= (argActions.indexOf('[usphone]') > -1)
	this.checkUSSSN		= (argActions.indexOf('[usssn]') > -1)

	if (argActions.indexOf('[len=') > -1) {
		this.checkLength = true

		var lenToCheck = ''
		var bolCont = true

		for (var intI=(argActions.indexOf('[len=') +  5);((intI < argActions.length) && bolCont); intI++)
			if (argActions.charAt(intI) != ']')
				lenToCheck += argActions.charAt(intI)
			else
				bolCont = false
		this.lengthToCheck = lenToCheck
	}
	else
		this.checkLength = false

	if (argActions.indexOf('[blankalert=') > -1) {
		this.blankAlert = true

		var alertString = ''
		var bolCont = true

		for (var intI=(argActions.indexOf('[blankalert=') +  12);((intI < argActions.length) && bolCont); intI++)
			if (argActions.charAt(intI) != ']')
				alertString += argActions.charAt(intI)
			else
				bolCont = false
		this.blankAlertMessage = alertString
	}
	else
		this.blankAlert = false
	
	if (argActions.indexOf('[invalidalert=') > -1) {
		this.invalidAlert = true

		var alertString = ''
		var bolCont = true

		for (var intI=(argActions.indexOf('[invalidalert=') +  14);((intI < argActions.length) && bolCont); intI++)
			if (argActions.charAt(intI) != ']')
				alertString += argActions.charAt(intI)
			else
				bolCont = false
		this.invalidAlertMessage = alertString
	}
	else
		this.invalidAlert = false

	if (argActions.indexOf('[equals=') > -1) {
		this.shouldEqual = true

		var equalsString = ''
		var bolCont = true

		for (var intI=(argActions.indexOf('[equals=') +  8);((intI < argActions.length) && bolCont); intI++)
			if (argActions.charAt(intI) != ']')
				equalsString += argActions.charAt(intI)
			else
				bolCont = false
		this.shouldEqualString = equalsString
	}
	else
		this.shouldEqual = false

}


//------------------------------------------------------------------------------------
// function: validateForm
//           Function to validate a HTML form
//------------------------------------------------------------------------------------
function validateForm(argForm)
	{
	var frmElements = argForm.elements
	var elemName
	var elemObj

	submitonce(argForm);

	for (var intI=0; intI < frmElements.length; intI++) {// *
		elemObj = frmElements[intI]
		elemName = elemObj.name

		if ((elemObj.type == 'hidden') && (elemName.length > 5))
			if (elemName.substr(elemName.length - 5).toLowerCase() == '_vldt') {// **
				var objAction = new actionFields(objectValue(frmElements, elemName))
				var actElem = elemName.substr(0, elemName.length - 5)
				
				// Is it a required field?
				if (objAction.required) {
					if (isWhiteSpace(objectValue(frmElements, actElem))) {// ***
						alert (objAction.blankAlert?objAction.blankAlertMessage:actElem + ' cannot be left blank')
						objectFocus(frmElements, actElem);
						submitenabled(argForm);
						return false
					} // ***
				}
				
				// Check the type validations
				if ((objectValue(frmElements, actElem) > '') && (!isWhiteSpace(objectValue(frmElements, actElem)))){// ***
					// Date
					if (objAction.checkDate)
						if (!isProperDate(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have an invalid date')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Number
					if (objAction.checkNumber)
						if (!isProperNumber(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have an invalid number')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Zip
					if (objAction.checkZip)
						if (!isProperZip(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have an invalid zipcode')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Alphabetic
					if (objAction.checkAlphabetic)
						if (!isProperAlphabetic(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have invalid characters')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Check US phone
					if (objAction.checkUSPhone)
						if (!isProperUSPhone(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have invalid characters')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Check US SSN
					if (objAction.checkUSSSN)
						if (!isProperUSSSN(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have invalid characters')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Check email
					if (objAction.email)
						if (!isProperEmail(objectValue(frmElements, actElem))) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' cannot have invalid characters')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****

					// Check for length
					if (objAction.checkLength)
						if (objectValue(frmElements, actElem).length < objAction.lengthToCheck) {// ****
							alert (objAction.invalidAlert?objAction.invalidAlertMessage:actElem + ' must be at least ' + objAction.lengthToCheck + ' characters long')
							objectFocus(frmElements, actElem);
							submitenabled(argForm);
							return false
						} // ****
				} // ***
			} // **
	} // *
		
	return true
}


function submitenabled(theform)
	{
	// if IE 4+ or NS 6+
	if (document.all||document.getElementById)
		{
		//screen thru every element in the form, and hunt down "submit" and "reset"
		for (i=0;i<theform.length;i++)
			{
			var tempobj=theform.elements[i];
			if(tempobj.type.toLowerCase()=="submit" || tempobj.type.toLowerCase()=="reset")
				//disable em
				tempobj.disabled=false;
			}
		}
	}


function submitonce(theform)
	{
	// if IE 4+ or NS 6+
	if (document.all||document.getElementById)
		{
		//screen thru every element in the form, and hunt down "submit" and "reset"
		for (i=0;i<theform.length;i++)
			{
			var tempobj=theform.elements[i];
			if(tempobj.type.toLowerCase()=="submit" || tempobj.type.toLowerCase()=="reset")
				//disable em
				tempobj.disabled=true;
			}
		}
	}
