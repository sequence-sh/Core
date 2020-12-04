grammar Sequence;
/*
 * Parser Rules
 */
setVariable			: VARIABLENAME EQUALS term;
getVariable         : VARIABLENAME ;
array				: OPENSQUAREBRACKET ( term ( COMMA term)* )? CLOSESQUAREBRACKET ;
infixOperator		: DASH
					| PLUS
					| ASTERIX
					| DIVIDE
					| PERCENT
					| CARROT
					| AND
					| OR
					| DOUBLEEQUALS
					| NOTEQUALS
					| LESSTHANEQUALS
					| GREATERTHENEQUALS
					| LESSTHAN
					| GREATERTHAN ;
infixOperation		: term infixOperator term ;
functionMember		: TOKEN (EQUALS | COLON) step ;
function			: TOKEN OPENBRACKET ( functionMember ( COMMA functionMember)* )? CLOSEBRACKET ;
entity				: OPENBRACKET ( functionMember ( COMMA functionMember)* )? CLOSEBRACKET ;
bracketedStep		: OPENBRACKET step CLOSEBRACKET ;
boolean				: TRUE | FALSE ;
quotedString		: DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING ;
number              : NUMBER ;
enumeration			: TOKEN DOT TOKEN ;
term				: simpleTerm
					| bracketedStep ;
step				: function
					| infixOperation
					| setVariable
					| term;
simpleTerm			: number
                    | boolean
                    | enumeration
                    | quotedString
                    | getVariable
                    | entity
                    | array ;
sequence			:(step | ((NEWCOMMAND | DASH) step (NEWCOMMAND step)*))  EOF ;


/*
 * Lexer Rules
 */

DASH                : '-' ;
PLUS                : '+' ;
ASTERIX             : '*' ;
DIVIDE              : '/' ;
PERCENT             : '%' ;
CARROT              : '^' ;
AND					: '&&' ;
OR					: '||' ;
DOUBLEEQUALS        : '==' ;
NOTEQUALS           : '!=' ;
LESSTHANEQUALS      : '<=' ;
GREATERTHENEQUALS   : '>=' ;
LESSTHAN            : '<' ;
GREATERTHAN         : '>' ;
EQUALS				: '=' ;
COLON				: ':' ;
OPENBRACKET			: '(' ;
CLOSEBRACKET		: ')' ;
OPENSQUAREBRACKET	: '[' ;
CLOSESQUAREBRACKET	: ']' ;
COMMA			    : ',' ;
NEWCOMMAND			: ('\r'? '\n' | '\r')+ (' ' | '\t')* DASH (' ' | '\t')+ ;
DOT                 : '.' ;
VARIABLENAME		: LESSTHAN [a-zA-Z0-9_]+ GREATERTHAN ;
NUMBER				: DASH? [0-9]+ ;
DOUBLEQUOTEDSTRING	: '"' (~('"' | '\\' | '\r' | '\n' | '\t') | '\\' ('"' | '\\' | 'r' | 'n' | 't'))* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
TOKEN				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n')+ -> skip ;