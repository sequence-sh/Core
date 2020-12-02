grammar Sequence;
/*
 * Parser Rules
 */
setvariable			: VARIABLENAME EQUALS member;
getvariable         : VARIABLENAME ;
array				: OPENSQUAREBRACKET ( member ( COMMA member)* )? CLOSESQUAREBRACKET ;
infixoperation		: member INFIXOPERATOR member ;
functionmember		: TOKEN EQUALS member ;
function			: TOKEN OPENBRACKET ( functionmember ( COMMA functionmember)* )? CLOSEBRACKET ;
entity				: OPENSQUAREBRACKET ( functionmember ( COMMA functionmember)* )? CLOSESQUAREBRACKET ;
bracketedoperation	: OPENSQUAREBRACKET infixoperation CLOSESQUAREBRACKET ;
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
sequence			: (NEWLINE)* member ((NEWLINE)+ member)* (NEWLINE)* EOF;


/*
 * Lexer Rules
 */

OPENBRACKET			: '(' ;
CLOSEBRACKET		: ')' ;
OPENSQUAREBRACKET	: '[' ;
CLOSESQUAREBRACKET	: ']' ;
COMMA			    : ',' ;
VARIABLENAME		: '<' [a-zA-Z0-9_]+ '>' ;
NUMBER				: '-'? [0-9]+ ;
INFIXOPERATOR		: ([+\-*/%^] | '&&' | '||' | '==' | '!=' | '<=' | '>=' | '<' | '>') ;
EQUALS				: ('=') ;
DOUBLEQUOTEDSTRING	: '"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\' | 'r' | 'n'))* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
TOKEN				: [a-zA-Z0-9_]+;
NEWLINE				: ('r'? 'n' | 'r')+ ;
DOT                 : '.' ;
WHITESPACE			: (' ' | '\t' | '\r' | '\n')+ -> skip ;