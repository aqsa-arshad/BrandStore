// ** I18N
// Bug fixed by AMSYS.it on 22/Jen/2006
// Calendar IT language
// Author: Mihai Bazon, <mishoo@infoiasi.ro>
// Encoding: any
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names
Calendar._DN = new Array
("Domenica",
 "Lunedi",
 "Martedi",
 "Mercoledi",
 "Giovedi",
 "Venerdi",
 "Sabato",
 "Domenica");

// Please note that the following array of short day names (and the same goes
// for short month names, _SMN) isn't absolutely necessary.  We give it here
// for exemplification on how one can customize the short day names, but if
// they are simply the first N letters of the full name you can simply say:
//
//   Calendar._SDN_len = N; // short day name length
//   Calendar._SMN_len = N; // short month name length
//
// If N = 3 then this is not needed either since we assume a value of 3 if not
// present, to be compatible with translation files that were written before
// this feature.

// short day names
Calendar._SDN = new Array
("Dom",
 "Lun",
 "Mar",
 "Mer",
 "Gio",
 "Ven",
 "Sab",
 "Dom");

// full month names
Calendar._MN = new Array
("Gennaio",
 "Febbraio",
 "Marzo",
 "Aprile",
 "Maggio",
 "Giugno",
 "luglio",
 "Agosto",
 "Settembre",
 "Ottobre",
 "Novembre",
 "Dicembre");

// short month names
Calendar._SMN = new Array
("Gen",
 "Feb",
 "Mar",
 "Apr",
 "Mag",
 "Giu",
 "Lug",
 "Ago",
 "Set",
 "Ott",
 "Nov",
 "Dic");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "About the calendar";

Calendar._TT["ABOUT"] =
"DHTML Date/Time Selector\n" +
"(c) dynarch.com 2002-2003\n" + // don't translate this this ;-)
"For latest version visit: http://dynarch.com/mishoo/calendar.epl\n" +
"Distributed under GNU LGPL.  See http://gnu.org/licenses/lgpl.html for details." +
"\n\n" +
"Date selection:\n" +
"- Use the \xab, \xbb buttons to select year\n" +
"- Use the " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " buttons to select month\n" +
"- Hold mouse button on any of the above buttons for faster selection.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"Time selection:\n" +
"- Click on any of the time parts to increase it\n" +
"- or Shift-click to decrease it\n" +
"- or click and drag for faster selection.";

Calendar._TT["PREV_YEAR"] = "Anno precedente ";
Calendar._TT["PREV_MONTH"] = "Mese precedente ";
Calendar._TT["GO_TODAY"] = "Vai ad Oggi";
Calendar._TT["NEXT_MONTH"] = "Prossimo Mese";
Calendar._TT["NEXT_YEAR"] = "Prossimo Anno ";
Calendar._TT["SEL_DATE"] = "Seleziona la Data";
Calendar._TT["DRAG_TO_MOVE"] = "Trascina";
Calendar._TT["PART_TODAY"] = " (oggi)";
Calendar._TT["MON_FIRST"] = "Mostra prima Lunedi";
Calendar._TT["SUN_FIRST"] = "Mostra prima Domenica";
Calendar._TT["CLOSE"] = "Chiudi";
Calendar._TT["TODAY"] = "Oggi";
Calendar._TT["TIME_PART"] = "(Shift-)Clicca o Trscina per cambiare Valore";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%d-%m-%Y";
Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";

Calendar._TT["WK"] = "wk";
