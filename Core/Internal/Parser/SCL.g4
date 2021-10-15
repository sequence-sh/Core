grammar SCL;
/*
 * Parser Rules
 */
setVariable			: VARIABLENAME EQUALS step;
getVariable         : VARIABLENAME ;
getAutomaticVariable : AUTOMATICVARIABLE ;
array				: OPENSQUAREBRACKET ( term COMMA? )* CLOSESQUAREBRACKET ;
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
					| GREATERTHAN;
infixOperation		: infixableTerm (infixOperator infixableTerm)+ ;
entityPropertyName  : NAME | quotedString;
entityProperty		: entityPropertyName (DOT entityPropertyName)* COLON term ;
namedArgument		: NAME COLON term ;
function			: NAME (term)* (namedArgument)* ;
entity				: OPENBRACKET (entityProperty COMMA?)*  CLOSEBRACKET ;
bracketedStep		: OPENBRACKET step CLOSEBRACKET ;
lambda              : OPENBRACKET (VARIABLENAME | AUTOMATICVARIABLE) ARROW step CLOSEBRACKET ;
boolean				: TRUE | FALSE ;
dateTime			: DATETIME ;
interpolatedString	: OPENISTRING step (ISTRINGSEGMENT step)* CLOSEISTRING;
quotedString		: DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING | SIMPLEISTRING ;
number              : NUMBER (DOT NUMBER)? ;
enumeration			: NAME DOT NAME ;
null                : NULL ;

infixableTerm       : simpleTerm #SimpleTerm1                                      
					
                    | arrayOrEntity=infixableTerm OPENSQUAREBRACKET indexer=term CLOSESQUAREBRACKET #ArrayAccess
                    | bracketedStep  #BracketedStep1   
                    ;

term				: infixableTerm   #InfixableTerm1
                    | function #Function1  
                    
                    | infixOperation #InfixOperation1
                    ;
step				: <assoc=right> step PIPE function #PipeFunction					
					| setVariable #SetVariable1
					| stepSequence #StepSequence1
					| term #Term1
					;
simpleTerm			: null
                    | number
                    | boolean
					| dateTime
                    | enumeration
                    | quotedString
					| interpolatedString
                    | lambda
                    | getVariable
                    | getAutomaticVariable
                    | entity
                    | array;
stepSequence		: (NEWCOMMAND | DASH) step (NEWCOMMAND step)* ;
fullSequence		: (step | stepSequence)  EOF ;


/*
 * Lexer Rules
 */
SINGLELINECOMMENT	: '#'  ~[\r\n\u0085\u2028\u2029]* -> channel(HIDDEN);
DELIMITEDCOMMENT	: '/*'  .*? '*/' -> channel(HIDDEN);
DASH                : '-' ;
PLUS                : '+' ;
ASTERIX             : '*' ;
DIVIDE              : '/' ;
PERCENT             : '%' ;
CARROT              : '^' ;
AND					: '&&' ;
OR					: '||' ;
ARROW               : '=>';
DOUBLEEQUALS        : '==' ;
NOTEQUALS           : '!=' ;
LESSTHANEQUALS      : '<=' ;
GREATERTHENEQUALS   : '>=' ;
LESSTHAN            : '<' ;
GREATERTHAN         : '>' ;
EQUALS				: '=' ;
COLON				: ':' ;
DOLLAR              : '$' ;
OPENBRACKET			: '(' ;
CLOSEBRACKET		: ')' ;
OPENSQUAREBRACKET	: '[' ;
CLOSESQUAREBRACKET	: ']' ;
OPENBRACE       	: '{' ;
CLOSEBRACE      	: '}' ;
COMMA			    : ',' ;
PIPE				: '|' ;
NEWCOMMAND			: ('\r'? '\n' | '\r')+ (' ' | '\t')* DASH (' ' | '\t')+ ;
DOT                 : '.' ;
fragment DIGIT		: [0-9];
AUTOMATICVARIABLE   : LESSTHAN GREATERTHAN ;
VARIABLENAME		: LESSTHAN [a-zA-Z0-9_]+ GREATERTHAN ;
DATETIME			: DIGIT DIGIT DIGIT DIGIT DASH DIGIT DIGIT DASH DIGIT DIGIT ([Tt] DIGIT DIGIT COLON DIGIT DIGIT COLON DIGIT DIGIT ('.' DIGIT+)?)? ;
NUMBER				: DASH? DIGIT+ ;
fragment ISTRINGCHAR: (~('"' | '\\' | '\r' | '\n' | '\t' | '{' | '}') | '\\' ('"' | '\\' | 'r' | 'n' | 't' | '{{' | '}}'));
OPENISTRING         : DOLLAR '"' ISTRINGCHAR* OPENBRACE;
ISTRINGSEGMENT		: CLOSEBRACE ISTRINGCHAR* OPENBRACE;
CLOSEISTRING		: CLOSEBRACE ISTRINGCHAR* '"';
SIMPLEISTRING		: DOLLAR '"' ISTRINGCHAR* '"';
fragment DQSCHAR	: (~('"' | '\\' | '\r' | '\n' | '\t') | '\\' ('"' | '\\' | 'r' | 'n' | 't'));
DOUBLEQUOTEDSTRING	: '"' DQSCHAR* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
NULL                : [Nn] [Uu] [Ll] [Ll];
NAME				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n') -> channel(HIDDEN) ;
