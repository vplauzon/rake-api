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

#	A reference is something that can readily be referenced
#	Either a value or a variable reference
rule reference = integer | quotedString | identifier;
rule parameterList = "(" h:expression t:("," e:expression)* ")";
rule emptyParameterList = "(" ")";
rule genericParameterList = e::emptyParameterList | p:parameterList;
rule genericMethodInvoke = "." i:identifier params:genericParameterList?;

#	An expression is a generic method call
rule(recursive=true) expression = r:reference methodInvokeList:genericMethodInvoke*;

rule main = expression;