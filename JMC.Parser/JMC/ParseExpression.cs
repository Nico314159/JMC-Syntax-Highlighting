﻿using JMC.Shared;
using JMC.Shared.Datas.BuiltIn;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// Parse a block
        /// </summary>
        /// <param name="index"></param>
        /// <returns><seealso cref="JMCSyntaxNode"/> has only <seealso cref="JMCSyntaxNode.Next"/></returns>
        private async Task<JMCParseResult> ParseBlockAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            while (index < TrimmedText.Length)
            {
                var exp = await ParseExpressionAsync(NextIndex(index));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}") break;
            }

            //set next
            node.Next = next.Count == 0 ? null : next;

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var text = TrimmedText[index];
            JMCParseResult? result = text switch
            {
                "do" => await ParseDoAsync(NextIndex(index)),
                "while" => await ParseWhileAsync(NextIndex(index)),
                "for" => await ParseForAsync(NextIndex(index)),
                "switch" => await ParseSwitchAsync(NextIndex(index)),
                "if" => await ParseIfAsync(NextIndex(index)),
                _ => null,
            };
            if (result != null)
                return result;
            var current = await ParseAsync(index);
            if (current.Node == null)
                return new(null, index, GetIndexStartPos(index));

            if (ExtensionData.CommandTree.RootCommands.Contains(text))
            {
                var cr = await ParseCommandExpressionAsync(index);
                if (cr?.Node != null)
                {
                    node.Next = cr.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION_COMMAND;
                    node.Range = cr.Node.Range;
                    index = cr.EndIndex;
                }
                return new(cr?.Node, index, GetIndexStartPos(index));
            }

            else if (current.Node.NodeType == JMCSyntaxNodeType.VARIABLE)
            {
                var r = await ParseVariableExpressionAsync(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index, GetIndexStartPos(index));
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.LITERAL &&
                TrimmedText[NextIndex(index)] == ":")
            {
                var r = await ParseScoreboardObjExpressionAsync(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index, GetIndexStartPos(index));
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.LITERAL &&
                TrimmedText[NextIndex(index)] == "(")
            {
                var r = await ParseFunctionCallAsync(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index, GetIndexStartPos(index));
            }
            return new(null, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// Parse Variable
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseVariableExpressionAsync(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var value = TrimmedText[index];
            //start position
            var startPos = GetIndexStartPos(index);
            //parse assignment
            var result = await ParseAssignmentExpressionAsync(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                var query = this.AsParseQuery(index);
                await query.ExpectAsync(JMCSyntaxNodeType.SEMI);
            }

            //end position
            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = value;
            node.NodeType = JMCSyntaxNodeType.VARIABLE;

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// Parse a scoreboard object
        /// </summary>
        /// <remarks>
        /// literal ':' selector
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseScoreboardObjExpressionAsync(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.COLON, JMCSyntaxNodeType.SELECTOR);
            var value = string.Join("", TrimmedText[index..(query.Index + 1)].Where(v => !string.IsNullOrEmpty(v)));
            index = query.Index;
            var startPos = GetIndexStartPos(index);
            //parse assignment
            var result = await ParseAssignmentExpressionAsync(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                query.Reset(this, index);
                await query.ExpectAsync(JMCSyntaxNodeType.SEMI);
            }


            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = JMCSyntaxNodeType.SCOREBOARD;
            node.Value = value;

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseAssignmentExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectOrAsync([.. OperatorsAssignTokens]);

            if (match.Item1)
            {
                next.Add((await ParseAsync(index)).Node!);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var r = await ParseAsync(index);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (await query.ExpectAsync(JMCSyntaxNodeType.VARIABLE, false))
                {
                    var r = await ParseVariableExpressionAsync(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (await query.ExpectAsync(JMCSyntaxNodeType.LITERAL, false))
                {
                    var r = await ParseScoreboardObjExpressionAsync(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
            }

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// parse a function call expression
        /// </summary>
        /// <remarks>
        /// '(' parameters* ')'
        /// </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseFunctionCallAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //parse parameters
            var functionValue = TrimmedText[index];
            index = NextIndex(index);
            var start = GetIndexStartPos(index);
            var param = await ParseParametersAsync(NextIndex(index), functionValue);

            //test for RPAREN
            index = param.EndIndex;
            var query = this.AsParseQuery(index);
            await query.ExpectAsync(JMCSyntaxNodeType.RPAREN);
            index = NextIndex(query.Index);

            //get end pos
            var end = GetIndexStartPos(index);

            node.Next = param.Node?.Next;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.FUNCTION_CALL;
            node.Value = functionValue;

            return new(node, index, end);
        }

        /// <summary>
        /// parse parameters in a function call
        /// </summary>
        /// <param name="index"></param>
        /// <param name="funcLiteral"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseParametersAsync(int index, string funcLiteral)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var split = funcLiteral.Split('.');
            var builtinFunc = ExtensionData.JMCBuiltInFunctions.GetFunction(split.First(), split.Last());
            while (TrimmedText[index] != ")")
            {
                var param = builtinFunc != null ? await ParseParameterAsync(index, builtinFunc) : await ParseParameterAsync(index);
                if (param.Node != null)
                    next.Add(param.Node);
                index = NextIndex(param.EndIndex);
                if (TrimmedText[index] == ",")
                    index = NextIndex(index);
            }

            node.Next = next.Count == 0 ? null : next;

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// parse a parameter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseParameterAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);
            // literal '=' (number || literal)
            await query.ExpectAsync(JMCSyntaxNodeType.LITERAL);
            next.Add(new(JMCSyntaxNodeType.LITERAL, TrimmedText[index], range: GetRangeByIndex(index)));
            index = NextIndex(query.Index);

            await query.Next().ExpectAsync(JMCSyntaxNodeType.EQUAL_TO);
            next.Add(new(JMCSyntaxNodeType.EQUAL_TO, TrimmedText[index], range: GetRangeByIndex(index)));
            index = NextIndex(query.Index);

            var r = await query.Next().ExpectOrAsync(JMCSyntaxNodeType.LITERAL, JMCSyntaxNodeType.NUMBER);
            next.Add(new(r.Item2 ?? JMCSyntaxNodeType.UNKNOWN, TrimmedText[index], range: GetRangeByIndex(index)));
            index = query.Index;
            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = JMCSyntaxNodeType.PARAMETER;
            node.Range = new(start, end);

            return new(node, index, GetIndexStartPos(index));
        }

        /// <summary>
        /// parse a parameter for built-in function
        /// </summary>
        /// <param name="index"></param>
        /// <param name="builtInFunction"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseParameterAsync(int index, JMCBuiltInFunction builtInFunction)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            //TODO: parse params

            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = JMCSyntaxNodeType.PARAMETER;
            node.Range = new(start, end);

            return new(node, index, GetIndexStartPos(index));
        }
    }
}
