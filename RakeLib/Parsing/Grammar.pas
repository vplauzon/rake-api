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

rule primitive = int:integer | string:quotedString | id:identifier;
rule property = obj:expression "." name:identifier;
rule emptyParameterList = "(" ")";
rule nonEmptyParameterList = "(" head:expression tail:("," e:expression)* ")";
rule parameterList = empty::emptyParameterList | nonEmpty:nonEmptyParameterList;
rule methodInvoke = obj:expression "." name:identifier params:parameterList;

rule(recursive=true) expression = prim:primitive | prop:property | meth:methodInvoke;

rule main = expression;