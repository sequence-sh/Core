grammar SCL;
/*
 * Parser Rules
 */
setVariable			: VARIABLENAME EQUALS step;
getVariable         : VARIABLENAME ;
getAutomaticVariable : AUTOMATICVARIABLE ;
array				: OPENSQUAREBRACKET ( term COMMA? )* CLOSESQUAREBRACKET ;
unbracketedArray	: ( sclObjectTerm COMMA )+ sclObjectTerm ;
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
number              : NUMBER ;
boolean				: TRUE | FALSE ;
dateTime			: DATETIME ;
interpolatedString	: OPENISTRING step? (ISTRINGSEGMENT step?)* CLOSEISTRING;
quotedString		: MULTILINESTRING | DOUBLEQUOTEDSTRING | SINGLEQUOTEDSTRING | SIMPLEISTRING ;
enumeration			: NAME DOT NAME ;
nullValue           : NULLVALUE ;

infixableTerm       : sclObjectTerm #SclObjectTerm1                                   					
                    | arrayOrEntity=infixableTerm OPENSQUAREBRACKET indexer=indexerTerm CLOSESQUAREBRACKET #ArrayAccess
                    | accessedEntity=infixableTerm DOT indexer=NAME #EntityGetValue
                    | bracketedStep  #BracketedStep1   
                    ;

indexerTerm         : infixableTerm
                    | function
                    | infixOperation 
                    ;


term				: infixableTerm   #InfixableTerm1
                    | function #Function1  
                    | unbracketedArray #UnbracketedArray1
                    | infixOperation #InfixOperation1
                    ;
step				: <assoc=right> step PIPE function #PipeFunction					
					| setVariable #SetVariable1
					| stepSequence #StepSequence1
					| term #Term1
					;
sclObjectTerm       : nullValue
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
                    | array
                    ;
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
NUMBER				: DASH? DIGIT+ (DOT DIGIT*)? ;
fragment ISTRINGCHAR: (~('"' | '\\' | '\r' | '\n' | '\t' | '{' | '}') | '\\' ('"' | '\\' | 'r' | 'n' | 't' | '{{' | '}}'));
OPENISTRING         : DOLLAR '"' ISTRINGCHAR* OPENBRACE;
ISTRINGSEGMENT		: CLOSEBRACE ISTRINGCHAR* OPENBRACE;
CLOSEISTRING		: CLOSEBRACE ISTRINGCHAR* '"';
SIMPLEISTRING		: DOLLAR '"' ISTRINGCHAR* '"';
fragment DQSCHAR	: (~('"' | '\\' | '\r' | '\n' | '\t') | '\\' ('"' | '\\' | 'r' | 'n' | 't'));
MULTILINESTRING     : '"''"''"' .*? '"''"''"' ;
DOUBLEQUOTEDSTRING	: '"' DQSCHAR* '"' ;
SINGLEQUOTEDSTRING	: '\'' (~('\'') | '\'\'')* '\'' ;
TRUE				: [Tt] [Rr] [Uu] [Ee];
FALSE				: [Ff] [Aa] [Ll] [Ss] [Ee];
NULLVALUE           : [Nn] [Uu] [Ll] [Ll];
NAME				: [a-zA-Z0-9_]+;
WHITESPACE			: (' ' | '\t' | '\r' | '\n') -> channel(HIDDEN) ;
