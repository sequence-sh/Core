grammar Sequence;
/*
 * Parser Rules
 */
setvariable			: VARIABLENAME EQUALS term;
getvariable         : VARIABLENAME ;
array				: OPENSQUAREBRACKET ( term ( COMMA term)* )? CLOSESQUAREBRACKET ;
infixoperator		: DASH
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
infixoperation		: term infixoperator term ;
functionmember		: TOKEN EQUALS step ;
function			: TOKEN OPENBRACKET ( functionmember ( COMMA functionmember)* )? CLOSEBRACKET ;
entity				: OPENBRACKET ( functionmember ( COMMA functionmember)* )? CLOSEBRACKET ;
bracketedstep		: OPENBRACKET step CLOSEBRACKET ;
bool				: TRUE | FALSE ;
string              : DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING ;
number              : NUMBER ;
enum                : TOKEN DOT TOKEN ;
term				: simpleterm
					| bracketedstep ;
step				: function
					| infixoperation
					| setvariable
					| term;
simpleterm			: number
                    | bool
                    | enum
                    | string
                    | getvariable
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
OPENBRACKET			: '(' ;
CLOSEBRACKET		: ')' ;
OPENSQUAREBRACKET	: '[' ;
CLOSESQUAREBRACKET	: ']' ;
COMMA			    : ',' ;
NEWCOMMAND			: ('\r'? '\n' | '\r')+ DASH (' ' | '\t') ;
DOT                 : '.' ;
VARIABLENAME		: LESSTHAN [a-zA-Z0-9_]+ GREATERTHAN ;
NUMBER				: DASH? [0-9]+ ;
DOUBLEQUOTEDSTRING	: '"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\' | 'r' | 'n'))* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
TOKEN				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n')+ -> skip ;