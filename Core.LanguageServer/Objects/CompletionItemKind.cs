namespace Reductech.Sequence.Core.LanguageServer.Objects;

public enum CompletionItemKind
{
    Text = 1,
    Method = 2,
    Function = 3,
    Constructor = 4,
    Field = 5,
    Variable = 6,
    Class = 7,
    Interface = 8,
    Module = 9,
    Property = 10,      // 0x0000000A
    Unit = 11,          // 0x0000000B
    Value = 12,         // 0x0000000C
    Enum = 13,          // 0x0000000D
    Keyword = 14,       // 0x0000000E
    Snippet = 15,       // 0x0000000F
    Color = 16,         // 0x00000010
    File = 17,          // 0x00000011
    Reference = 18,     // 0x00000012
    Folder = 19,        // 0x00000013
    EnumMember = 20,    // 0x00000014
    Constant = 21,      // 0x00000015
    Struct = 22,        // 0x00000016
    Event = 23,         // 0x00000017
    Operator = 24,      // 0x00000018
    TypeParameter = 25, // 0x00000019
}
