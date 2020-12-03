grammar Sequence;
/*
 * Parser Rules
 */
setvariable			: VARIABLENAME EQUALS member;
getvariable         : VARIABLENAME ;
array				: OPENSQUAREBRACKET ( member ( COMMA member)* )? CLOSESQUAREBRACKET ;
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
infixoperation		: member infixoperator member ;
functionmember		: TOKEN EQUALS member ;
function			: TOKEN OPENBRACKET ( functionmember ( COMMA functionmember)* )? CLOSEBRACKET ;
entity				: OPENSQUAREBRACKET ( functionmember ( COMMA functionmember)* )? CLOSESQUAREBRACKET ;
bracketedoperation	: OPENBRACKET infixoperation CLOSEBRACKET ;
bool				: TRUE | FALSE ;
string              : DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING ;
number              : NUMBER ;
enum                : TOKEN DOT TOKEN ;
member				: number
                    | bool
                    | enum
                    | string
                    | getvariable
                    | function
                    | entity
                    | bracketedoperation
                    | array  ;
sequence			:(member | (NEWCOMMAND | DASH) member (NEWCOMMAND member)*)  EOF ;


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
NEWCOMMAND			: ('\r'? '\n' | '\r')+ DASH ;
DOT                 : '.' ;
VARIABLENAME		: LESSTHAN [a-zA-Z0-9_]+ GREATERTHAN ;
NUMBER				: DASH? [0-9]+ ;
DOUBLEQUOTEDSTRING	: '"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\' | 'r' | 'n'))* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
TOKEN				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n')+ -> skip ;