using System;

namespace VpServiceAPI.Tools
{
    public class XMLParser
    {
        public string? XML { get; set; }
        public XMLParser(string xml)
        {
            XML = xml;
        }
        public XMLParser ExtractNode(string nodeTag)
        {
            XML = GetNode(XML, nodeTag);
            return this;
        }
        public XMLParser ExtractNodeContent(string nodeTag)
        {
            XML = GetNodeContent(XML, nodeTag);
            return this;
        }


        public static string? GetNode(string xml, string nodeTag)
        {
            try
            {

                int startIdx = xml.IndexOf($"<{nodeTag}");
                int endIdx;

                int openTagsCount = 1;
                int offset = startIdx + 1;
                while (true)
                {
                    int start = xml.IndexOf($"<{nodeTag}", offset);
                    int end = xml.IndexOf($"</{nodeTag}>", offset);

                    if(start < end && start != -1)
                    {
                        openTagsCount++;
                        offset = start + 1;
                        continue;
                    }
                    if(end < 0)
                    {
                        endIdx = xml.Length;
                        break;
                    }
                    openTagsCount--;
                    offset = end + 1;
                    if(openTagsCount == 0)
                    {
                        endIdx = end;
                        break;
                    }
                }
                return xml[startIdx..(endIdx + nodeTag.Length + 3)];
            }
            catch
            {
                return null;
            }
        }
        public static string? GetNodeContent(string xml, string nodeTag)
        {
            var node = GetNode(xml, nodeTag);
            if (node is null) return null;
            return node[(node.IndexOf('>') + 1)..^(nodeTag.Length + 3)];
        }

        public override string ToString()
        {
            return XML;
        }
    }
}
