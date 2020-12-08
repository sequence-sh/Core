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
namedArgument		: NAME COLON term ;
function			: NAME ( namedArgument )* ;
entity				: OPENBRACKET ( namedArgument )*  CLOSEBRACKET ;
bracketedStep		: OPENBRACKET step CLOSEBRACKET ;
boolean				: TRUE | FALSE ;
quotedString		: DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING ;
number              : NUMBER ;
enumeration			: NAME DOT NAME ;
term				: simpleTerm
					| bracketedStep ;
step				: function
					| infixOperation
					| setVariable
					| stepSequence
					| term;
simpleTerm			: number
                    | boolean
                    | enumeration
                    | quotedString
                    | getVariable
                    | entity
                    | array ;
stepSequence		: (NEWCOMMAND | DASH) step (NEWCOMMAND step)* ;
fullSequence		: (step | stepSequence)  EOF ;


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
NAME				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n') -> skip ;