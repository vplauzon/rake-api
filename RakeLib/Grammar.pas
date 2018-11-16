#	Comments & interleaves
rule(interleave=false) comment = "#" (. - ("\r" | "\n"))*;
interleave = (" " | "\r" | "\n" | "\t") | comment;

#	Primitives
rule(interleave=false, children=false) integer = ("0".."9")+;
rule(interleave=false) character = normal::(. - ("\"" | "\r" | "\n" | "\\"))
	| escapeQuote::("\\" "\"") | escapeBackslash::"\\\\"
	| escapeLetter:("\\" l::("n" | "r" | "t" | "v"))
	| escapeHexa:("\\x" h::("0".."9" | "a".."f" | "A".."F"){1,2});
rule(interleave=false) quotedString = "\"" s:character* "\"";

#	Identifiers
rule(interleave=false, children=false) letter = ("A".."Z") | ("a".."z");
rule(interleave=false, children=false) underscore = "_";
rule(interleave=false, children=false) identifier = (letter | underscore) (letter | underscore | integer)*;

#	Objects
rule nonEmptyParameterList = h:expression t:("," e:expression)*;
rule parameterList = n:nonEmptyParameterList | "";
rule bacedParameterList = "(" l:parameterList ")";
rule method = e:expression "." m:identifier l:bacedParameterList?;

#	Expression
rule(recursive=true) expression = integer | quotedString | identifier | method;

rule main = expression;