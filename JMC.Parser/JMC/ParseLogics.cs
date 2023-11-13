﻿namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        //TODO all not finished
        private async Task<JMCParseResult> ParseDoAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.Next().ExpectAsync(JMCSyntaxNodeType.LCP);
            var start = GetIndexStartPos(query.Index);

            //TODO:

            var end = GetIndexStartPos(query.Index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.DO;

            return new(node, index, GetIndexStartPos(index));
        }

        private async Task<JMCParseResult> ParseWhileAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, GetIndexStartPos(index));
        }

        private async Task<JMCParseResult> ParseForAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, GetIndexStartPos(index));
        }

        private async Task<JMCParseResult> ParseSwitchAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, GetIndexStartPos(index));
        }

        private async Task<JMCParseResult> ParseIfAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, GetIndexStartPos(index));
        }
    }
}
