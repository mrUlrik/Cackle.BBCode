using Cackle.BBCode.Internal;

namespace Cackle.BBCode;

/// <summary>
///     Provides the ability to parse a string with optional BBCode to a collection of <see cref="BBCodeNode" />.
/// </summary>
/// <remarks>
///     Initializes the <see cref="BBCodeParser" /> class.
/// </remarks>
/// <param name="allowedCodes">Collection of allowed BBCodes.</param>
public class BBCodeParser(IEnumerable<string> allowedCodes)
{
    private readonly HashSet<string> _allowedCodes = allowedCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Parse a string of text that may or may not contain BBCode.
    /// </summary>
    /// <param name="text">String to parse.</param>
    /// <returns>A collection of <see cref="BBCodeNode" /> that contains the text and associated codes.</returns>
    public IEnumerable<BBCodeNode> Parse(string text)
    {
        var nodes = new List<BBCodeNode>();

        // We're using a List instead of a HashSet because a HashSet will not allow duplicates. Though in reality, duplicate
        // nested tags can be entered by the user. We want to account for each.
        var activeCodes = new List<BBCode>();
        
        // Start with the Text stage. The incoming string doesn't have to have any codes in it anyway.
        var state = ParserState.Text;

        var reader = new StringBuffer(text);
        var node = new BBCodeNode(0);

        // Read until there is nothing left to read.
        while (reader.Read())
            switch (state)
            {
                // The only thing that can change our state is a '['.
                case ParserState.Text:
                {
                    // We've got a [ right out of the gate. Let's jump straight to OpeningTag stage.
                    if (reader.Token == '[')
                    {
                        state = ParserState.OpeningTag;
                        break;
                    }

                    while (reader.Token != '[')
                    {
                        node.Length++;
                        if (!reader.Read()) break;
                    }

                    // The StartIndex and Length have been built in other stages of this loop.
                    node.Content = reader.GetValue(node.StartIndex, node.Length);

                    // Apply any active codes to this node, in reverse because BBCodeNode.Codes will not skip codes with the
                    // same if already applied. i.e. Apply only the latest code with a specific name.
                    for (var i = activeCodes.Count - 1; i >= 0; i--) node.Codes.Add(activeCodes[i]);

                    // The Text and ClosingTag stages are the last stages. Add to the list and start a fresh node.
                    nodes.Add(node);
                    node = new BBCodeNode(reader.Index);

                    state = ParserState.OpeningTag;
                    break;
                }

                // We hit a '['. The only thing that can change our state is a ']' or '/'.
                case ParserState.OpeningTag:
                {
                    // What we thought was an opening code is actually a closing code.
                    if (reader.Token == '/')
                    {
                        state = ParserState.ClosingTag;
                        break;
                    }

                    var codePos = reader.Index;
                    var codeLen = 0;

                    // Something a little special here. We're looking for the end of a code, but also checking to see if it's
                    // got an attribute which is prefixed by a '='.
                    while (reader.Token != ']' && reader.Token != '=')
                    {
                        codeLen++;
                        if (!reader.Read()) break;
                    }

                    var codeName = reader.GetValue(codePos, codeLen);
                    if (_allowedCodes.Contains(codeName))
                    {
                        var code = new BBCode(codeName);

                        // Let's see if we found an attribute.
                        if (reader.Token == '=')
                            // Let's progress to the next token here since we won't be changing states. But we have to make sure
                            // we still have something to read in case the '=' is the last character in the buffer.
                            if (reader.Read())
                            {
                                var attrPos = reader.Index;
                                var attrLen = 0;

                                while (reader.Token != ']')
                                {
                                    attrLen++;
                                    if (!reader.Read()) break;
                                }

                                code.AttributeValue = reader.GetValue(attrPos, attrLen);
                            }

                        // Add this code the activeCodes list.
                        activeCodes.Add(code);

                        // We've got a valid code here and don't want it captured as content. The Text stage handles populating
                        // the Content field, so we are manipulating the StartIndex and Length here.

                        // The reader's position is at the last character in the code. Adding a + 1 to accomodate the ']'.
                        node.StartIndex = reader.Index + 1;

                        // We have not measured any content yet, so we're setting this to zero.
                        node.Length = 0;
                    }
                    else
                    {
                        // We do not have a valid code and do want it captured as content. The Text stage handles populating
                        // the Content field, so we are manipulating the StartIndex and Length here.

                        // The reader's position is at the last character in the code. Subtracting the length of the code from
                        // the current position plus an additional position to accomodate the opening '['.
                        node.StartIndex = reader.Index - codeLen - 1;

                        // We want this invalid code to be part of the content, so we're adding its length plus two positions
                        // to accomodate the '[' and ']'.
                        node.Length = codeLen + 2;
                    }

                    state = ParserState.Text;
                    break;
                }

                // We've finally reached a closing tag. The only thing that can change our state is a ']'.
                case ParserState.ClosingTag:
                {
                    var codePos = reader.Index;
                    var codeLen = 0;

                    while (reader.Token != ']')
                    {
                        codeLen++;
                        if (!reader.Read()) break;
                    }

                    var codeName = reader.GetValue(codePos, codeLen);
                    if (_allowedCodes.Contains(codeName))
                    {
                        // We've got a valid code here and don't want it captured as content. We are manipulating the StartIndex and
                        // Length here to avoid it being caught at the end of this stage.

                        // The reader's position is at the last character in the code. Adding a + 1 to accomodate the ']'.
                        node.StartIndex = reader.Index + 1;

                        // We have not measured any content yet, so we're setting this to zero.
                        node.Length = 0;

                        // If this code is currently in play, remove it.
                        var activeIndex = activeCodes.FindIndex(r => r.Name == codeName);
                        if (activeIndex != -1) activeCodes.RemoveAt(activeIndex);
                    }
                    else
                    {
                        // Like the OpeningTag stage, we do not have a valid code therefore we want it to appear in the
                        // Content during the Text stage.

                        // The reader's position is at the last character in the code. Subtracting the length of the code from
                        // the current position plus two additional position to accomodate the opening '[/'.
                        node.StartIndex = reader.Index - codeLen - 2;

                        // We want this invalid code to be part of the content, so we're adding its length plus three positions
                        // to accomodate the '[/' and ']'.
                        node.Length = codeLen + 3;

                        // This is an edge case. 1) if this is the last tag in the buffer, and 2) if it's a closing tag, and 3)
                        // it's last character is the final character in the buffer, and 4) it's not a valid code, the loop will
                        // end before the OpeningTag stage can add it to the Content. So we will check to see if we are at the
                        // end of the buffer and add it, if so.
                        if (reader.Peek() == char.MinValue)
                        {
                            // We hit an event where there was an invalid tag at the end of the string: [/sub[
                            // The caused the buffer to read to the end of the string, which means we run over if we simply read
                            // from node.StartIndex to node.Length.

                            // We've confirmed were at the end of the road with .Peek, so let's not think too hard about this.
                            node.Length = reader.Length - node.StartIndex;

                            node.Content = reader.GetValue(node.StartIndex, node.Length);
                            nodes.Add(node);
                            break;
                        }
                    }

                    // We don't have to capture closing tags because frankly, all we need from them is their position. The next
                    // stage will start at the proper position (and length, if we spotted an invalid tag).
                    node = new BBCodeNode(node.StartIndex, node.Length);

                    state = ParserState.Text;
                    break;
                }
            }

        return nodes;
    }
}