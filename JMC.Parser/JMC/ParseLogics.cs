﻿using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using JMC.Shared;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseCondition(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            while (true)
            {
                var isVar = query.Expect(JMCSyntaxNodeType.VARIABLE, out var sn, false);

                string[] commands = [.. ExtensionData.CommandTree.Nodes["execute"].Children!["if"].Children!.Keys];
                var isCommand = commands.Contains(TrimmedText[index]);

                //parse variable
                if (isVar)
                {
                    next.Add(sn!);
                    index = NextIndex(index);
                    var r = ParseVariableCondition(index);
                    if (r.Node?.Next != null)
                        next.AddRange(r.Node.Next);
                    index = NextIndex(r.EndIndex);
                }
                else if (isCommand)
                {
                    //TODO: support for command condition
                }


                query.Reset(index);
                if (!query.ExpectOr(out var syntaxNode, [.. LogicOperatorTokens]) ||
                    syntaxNode?.NodeType == JMCSyntaxNodeType.RPAREN)
                    break;
                else if (syntaxNode?.NodeType == JMCSyntaxNodeType.LPAREN)
                {
                    index = NextIndex(index);
                    var r = ParseCondition(index);
                    index = NextIndex(r.EndIndex);
                    if (r.Node != null)
                        next.Add(r.Node);
                }
                else
                {
                    next.Add(syntaxNode!);
                    query.Next();
                    index = NextIndex(index);
                }

            }

            var end = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.CONDITION;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseVariableCondition(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            if (query.ExpectOr(out var syntaxNode, [.. ConditionalOperatorTokens]))
            {
                next.Add(syntaxNode!);
                JMCSyntaxNodeType[] types = [JMCSyntaxNodeType.NUMBER, JMCSyntaxNodeType.VARIABLE, JMCSyntaxNodeType.SCOREBOARD];
                var match = query.Next().
                    ExpectOr(out syntaxNode, types);

                if (match && syntaxNode != null)
                    next.Add(syntaxNode!);
                else
                    Errors.Add(new JMCSyntaxError(GetIndexStartPos(index), TrimmedText[index], types));

                index = query.Index;
            }
            else if (query.Expect("matches", out syntaxNode, false))
            {
                next.Add(syntaxNode!);
                var match = query.Next().ExpectIntRange();
                index = query.Index;
                var range = GetRangeByIndex(index);
                next.Add(new(JMCSyntaxNodeType.INT_RANGE, TrimmedText[index], range: range));
            }
            else if (!query.Expect(JMCSyntaxNodeType.RPAREN, out _))
            {
                Errors.Add(new JMCSyntaxError(start, TrimmedText[index],
                    ConditionalOperatorTokens.Select(v => v.ToTokenString())
                    .Concat(["matches"])
                    .ToArray()));
            }

            var end = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.CONDITION;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseDo(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            //parse block
            var block = ParseBlock(index);
            if (block.Node != null)
                next.Add(block.Node);
            index = NextIndex(block.EndIndex);
            var query = this.AsParseQuery(index);

            //parse while
            query.Expect(JMCSyntaxNodeType.WHILE, out _);
            index = NextIndex(NextIndex(query.Index));

            //parse condition
            var condition = ParseCondition(index);
            if (condition.Node != null)
                next.Add(condition.Node);
            index = condition.EndIndex;

            var end = GetIndexStartPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.DO;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseWhile(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            //parse condition
            var condition = ParseCondition(NextIndex(index));
            if (condition.Node != null)
                next.Add(condition.Node);
            index = NextIndex(condition.EndIndex);

            //parse block
            var block = ParseBlock(index);
            if (block.Node != null)
                next.Add(block.Node);
            index = block.EndIndex;

            var end = GetIndexStartPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.WHILE;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseFor(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            //parse statement 1
            var s1 = ParseExpression(NextIndex(index));
            if (s1.Node != null)
                next.Add(s1.Node);
            index = s1.EndIndex;

            //parse statement 2
            var s2 = ParseCondition(NextIndex(index));
            if (s2.Node != null)
                next.Add(s2.Node);
            index = s2.EndIndex;

            //parse statement 3
            var s3 = ParseExpression(NextIndex(index), true);
            if (s3.Node != null)
                next.Add(s3.Node);
            index = s3.EndIndex;

            //parse block
            var block = ParseBlock(NextIndex(NextIndex(index)));
            if (block.Node != null)
                next.Add(block.Node);
            index = block.EndIndex;

            var end = GetIndexEndPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.FOR;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseSwitch(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //parse paren
            var start = GetIndexEndPos(index);
            var query = this.AsParseQuery(index);
            query.ExpectList(out var nodes, true, JMCSyntaxNodeType.VARIABLE, JMCSyntaxNodeType.RPAREN, JMCSyntaxNodeType.LCP);
            index = query.Index;

            //add variable
            next.Add(nodes.First());

            //get next text
            index = NextIndex(index);

            var texts = TrimmedText.AsSpan();
            ref var currentText = ref texts[index];
            //parse cases
            while (currentText != "}" && index < TrimmedText.Length)
            {
                //parse case
                var cr = ParseCase(index);
                if (cr.Node  != null)
                    next.Add(cr.Node);
                index = cr.EndIndex;
                currentText = ref texts[index]!;
            }

            var end = GetIndexEndPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.SWITCH;
            node.Value = nodes.FirstOrDefault()?.Value;

            return new(node, index);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseCase(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);
            var query = this.AsParseQuery(index);

            //test for syntax
            var match = 
                query.ExpectList(out var syntaxNodes, true, JMCSyntaxNodeType.NUMBER, JMCSyntaxNodeType.COLON);

            index = NextIndex(query.Index);

            var texts = TrimmedText.AsSpan();
            ref var currentText = ref texts[index];
            //parse cases
            while (currentText != "case" && currentText != "}" && index < TrimmedText.Length)
            {
                //parse case
                var cr = ParseExpression(index);
                if (cr.Node != null)
                    next.Add(cr.Node);
                index = NextIndex(cr.EndIndex);
                currentText = ref texts[index]!;
            }

            var end = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.CASE;
            node.Value = syntaxNodes.FirstOrDefault()?.Value;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseIf(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            //parse condition
            var condition = ParseCondition(NextIndex(index));
            if (condition.Node != null)
                next.Add(condition.Node);
            index = condition.EndIndex;

            //parse block
            var block = ParseBlock(index);
            index = block.EndIndex;
            if (block.Node != null)
                next.Add(block.Node);

            var end = GetIndexEndPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.NodeType = JMCSyntaxNodeType.IF;
            node.Range = new(start, end);

            return new(node, index);
        }
    }
}
