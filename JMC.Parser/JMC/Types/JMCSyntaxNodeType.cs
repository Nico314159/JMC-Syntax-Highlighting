﻿namespace JMC.Parser.JMC.Types
{
    public enum JMCSyntaxNodeType
    {
        UNKNOWN,
        LCP,
        RCP,
        FUNCTION,
        CLASS,
        COMMENT,
        NUMBER,
        STRING,
        MULTILINE_STRING,
        VARIABLE,
        VARIABLE_CALL,
        SELECTOR,
        VEC2,
        VEC3,
        TILDE,
        CARET,
        LPAREN,
        RPAREN,
        IMPORT,
        SEMI,
        LITERAL,
        TRUE,
        FALSE,
        OPERATOR,
        COLON,
        FOR,
        DO,
        WHILE,
        OP_INCREMENT,
        OP_DECREMENT,
        OP_SUBSTRACT,
        OP_PLUS,
        OP_MULTIPLY,
        OP_DIVIDE,
        OP_PLUSEQ,
        OP_DIVIDEEQ,
        OP_MULTIPLYEQ,
        OP_SUBSTRACTEQ,
        OP_SWAP,
        OP_NULLCOALE,
        COMP_OR,
        COMP_NOT,
        OP_SUCCESS,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_EQ,
        LESS_THAN_EQ,
        EQUAL_TO,
        EQUAL,
        EXPRESSION,
        SCOREBOARD,
        EXPRESSION_COMMAND,
        COMMAND_STRING,
        BOOL,
        COMMAND_DOUBLE,
        DOUBLE,
        FLOAT,
        INT,
        LONG,
        ANGLE,
        BLOCK_STATE,
        COLOR,
        ENTITY,
        ENTITY_ANCHOR,
        COMMAND_FUNCTION,
        FLOAT_RANGE,
        FUNCTION_CALL,
        INT_RANGE,
        ITEM_PREDICATE,
        ITEM_SLOT,
        ITEM_STACK,
        MESSAGE,
        NBT,
        NBT_PATH,
        NBT_TAG,
        OBJECTIVE_CRITERIA,
        PARTICLE,
        RESOURCE,
        RESOURCE_TAG,
        ROTATION,
        TEAM,
        TIME,
        UUID,
        COMMAND,
        PARAMETER,
        ARROW_FUNCTION,
        COMP_AND,
        CONDITION,
        IF,
        BLOCK,
        RANGE,
        BREAK,
        SWITCH,
        CASE,
    }
}
