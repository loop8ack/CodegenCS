﻿using CodegenCS.___InternalInterfaces___;
using System.Collections.Generic;

namespace CodegenCS
{

    #region InlineIEnumerable<T>
    /// <summary>
    /// This is just a wrapper to pass IEnumerable{T} with custom <see cref="RenderEnumerableOptions"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InlineIEnumerable<T> : IInlineIEnumerable
    {
        public IEnumerable<T> Items { get; private set; }
        public RenderEnumerableOptions RenderOptions { get; private set; } = null; // if not set will follow ICodegenTextWriter.DefaultIEnumerableRenderOptions

        public InlineIEnumerable(IEnumerable<T> items)
        {
            Items = items;
        }
        public InlineIEnumerable(IEnumerable<T> items, RenderEnumerableOptions renderOptions)
        {
            Items = items;
            RenderOptions = renderOptions;
        }
        object IInlineIEnumerable.Items => Items;

    }
    #endregion

    #region IEnumerableExtensions
    /// <summary>
    /// One of the major features of ICodegenTextWriter is that it accepts FormattableStrings (interpolated strings) everywhere and it can render a large number of object types that you can just interpolate. <br />
    /// IEnumerables of FormattableString, Func{FormattableString}, string, or Func{string} can be embedded directly and they will be automatically rendered by ICodegenTextWriter which handles not only writing
    /// the different items (while handling indentation as usual) but also how the items should be separated from each other.
    /// The Render() extensions below allow users to specify custom rendering options to adjust how items should be separated, how last line should be handled, etc.
    /// </summary>
    public static class IEnumerableExtensions
    {
        #region Render() extensions

        /// <summary>
        /// Renders the items using the default configuration from <see cref="ICodegenTextWriter.DefaultIEnumerableRenderOptions"/> for describing how the items should be separated.
        /// Default configuration is good for rendering single-line or multiline blocks: 
        /// - Between the items it ensures a line spacer (a full empty line between the items) if the item has multiple lines, or will just ensure a linebreak if item was a single line
        /// - If the last item does not end with a line break (ends in a dirty line) it ensures that any further write will go to the next line (force a linebreak only if/when required)
        /// If you don't want line spacers between the items (useful if you're writing single line items) use the <see cref="RenderEnumerableOptions.LineBreaksWithoutSpacer"/>
        /// If you just interpolate the IEnumerable{T} without invoking this Render() extension the items will also be rendered (as if Render() was invoked) using the default options.
        /// If you interpolate an IEnumerable without using the Render() extension it will also follow the default configuration from <see cref="ICodegenTextWriter.DefaultIEnumerableRenderOptions"/>
        /// </summary>
        public static InlineIEnumerable<T> Render<T>(this IEnumerable<T> items)
        {
            return new InlineIEnumerable<T>(items);
        }


        /// <summary>
        /// Renders the items by specifying custom rendering options that define how items are separated.
        /// For presets check the static members of <see cref="RenderEnumerableOptions"/> (e.g. <see cref="RenderEnumerableOptions.LineBreaksWithAutoSpacer"/>, etc)
        /// </summary>
        public static InlineIEnumerable<T> Render<T>(this IEnumerable<T> items, RenderEnumerableOptions customRenderOptions)
        {
            
            return new InlineIEnumerable<T>(items, customRenderOptions);
        }


        /// <summary>
        /// Renders items by separating them with line breaks (this is probably what you want). 
        /// </summary>
        /// <param name="useLineSpacer">If true (default) it will render an empty line (spacer) between the items</param>
        public static InlineIEnumerable<T> RenderWithLineBreaks<T>(this IEnumerable<T> items, bool useLineSpacer = true) 
            => new InlineIEnumerable<T>(items, useLineSpacer ? RenderEnumerableOptions.LineBreaksWithSpacer : RenderEnumerableOptions.LineBreaksWithoutSpacer);


        /// <summary>
        /// Renders single-line items by joining them with commas. Don't add linebreak after last item.
        /// </summary>
        public static InlineIEnumerable<T> RenderAsSingleLineCSV<T>(this IEnumerable<T> items) => new InlineIEnumerable<T>(items, RenderEnumerableOptions.SingleLineCSV);


        #endregion

    }

    #endregion


    #region IEnumerableRenderOptions
    public class RenderEnumerableOptions
    {
        public string CustomSeparator { get; set; } = ", ";

        /// <summary>
        /// What will be written after each enumerable item (except the last one)
        /// Default value is <see cref="ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite"/>. Other common option is <see cref="ItemsSeparatorBehavior.EnsureFullEmptyLine"/>
        /// </summary>
        public ItemsSeparatorBehavior BetweenItemsBehavior { get; set; } = ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite;

        /// <summary>
        /// What will be written after the last enumerable item. 
        /// Default value is <see cref="ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite"/>.
        /// </summary>
        public ItemsSeparatorBehavior AfterLastItemBehavior { get; set; } = ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite;

        public RenderEnumerableOptions()
        {
        }

        public static RenderEnumerableOptions CreateWithCustomSeparator(string customSeparator, bool singleLine = false) => new RenderEnumerableOptions()
        {
            CustomSeparator = customSeparator,
            BetweenItemsBehavior = ItemsSeparatorBehavior.WriteCustomSeparator,
            AfterLastItemBehavior = singleLine ? ItemsSeparatorBehavior.None : ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite,
        };

        #region Some predefined settings
        /// <summary>
        /// Between the items will ensure that there's a line break (meaning that it will add a linebreak unless the item explicitly wrote a linebreak at the end). (default behavior)
        /// After the last item it won't write anything but will ensure that next write gets a linebreak (default behavior).
        /// </summary>
        public static RenderEnumerableOptions LineBreaksWithoutSpacer => new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.EnsureLineBreak };

        /// <summary>
        /// Between the items will ensure that there's a line spacer (a full empty line - good for isolating multiline blocks)
        /// After the last item it won't write anything but will ensure that next write gets a linebreak (default behavior).
        /// </summary>
        public static RenderEnumerableOptions LineBreaksWithSpacer => new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.EnsureFullEmptyLine };

        /// <summary>
        /// Between the items will ensure that there's a line spacer (a full empty line) only if the previous item wrote more than a single line, else it will just add ensure a simple linebreak.
        /// After the last item it won't write anything but will ensure that next write gets a linebreak (default behavior).
        /// (This is the default option and the recommendation for most cases)
        /// </summary>
        public static RenderEnumerableOptions LineBreaksWithAutoSpacer => new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.EnsureFullEmptyLineAfterMultilineItems };

        /// <summary>
        /// Between the items will write commas (no linebreaks).
        /// After the last item won't write anything (no linebreaks or anything).
        /// </summary>
        public static RenderEnumerableOptions SingleLineCSV => RenderEnumerableOptions.CreateWithCustomSeparator(", ", singleLine: true);

        /// <summary>
        /// Between the items will write a comma and linebreak. 
        /// After the last item it won't write anything but will ensure that next write gets a linebreak (default behavior).
        /// </summary>
        public static RenderEnumerableOptions MultiLineCSV => RenderEnumerableOptions.CreateWithCustomSeparator(",\n");

        /// <summary>
        /// Between the items will write a comma and two linebreaks (which means a full empty line between items)
        /// After the last item it won't write anything but will ensure that next write gets a linebreak (default behavior).
        /// </summary>
        public static RenderEnumerableOptions SpacedMultiLineCSV => RenderEnumerableOptions.CreateWithCustomSeparator(",\n\n");


        #endregion
    }

    #endregion

    #region ItemsSeparatorBehavior
    public enum ItemsSeparatorBehavior
    {
        /// <summary>
        /// Writes a line break after writing the item
        /// </summary>
        WriteLineBreak,

        /// <summary>
        /// If the current line is dirty (previous item did not end in a line break) it will write a line break, ensuring that the next line is empty.
        /// Useful because if the previous item ends with linebreak we don't need to force a new one.
        /// </summary>
        EnsureLineBreak,

        /// <summary>
        /// If the current line is dirty (previous item did not end in a line break) it will NOT force a line break, 
        /// but yet it will ensure that the next write will enforce (render automatically) a line break.
        /// In other words this signals that the current dirty line can not have any further text appended to the same line.
        /// This is helpful because when we embed IEnumerables inside Templates we usually already have a linebreak after the IEnumerable, 
        /// and therefore if <see cref="RenderEnumerableOptions.AfterLastItemBehavior"/> is set to <see cref="ItemsSeparatorBehavior.WriteLineBreak"/> or <see cref="ItemsSeparatorBehavior.EnsureLineBreak"/>
        /// we would have two linebreaks (meaning an empty line)
        /// </summary>
        EnsureLineBreakBeforeNextWrite,

        /// <summary>
        /// Ensures that there's at least a full empty line after the item. 
        /// This is like <see cref="ItemsSeparatorBehavior.EnsureLineBreak"/> (ensures a line break after a dirty line) and on top of that it writes a full empty line, good for isolation between large blocks.
        /// E.g. When writing large classes and setting <see cref="RenderEnumerableOptions.BetweenItemsBehavior"/> to <see cref="ItemsSeparatorBehavior.EnsureFullEmptyLine"/> we enforce some isolation between the blocks.
        /// </summary>
        EnsureFullEmptyLine,

        /// <summary>
        /// Ensures that there's at least a full empty line after the item but only if item wrote more than a single line
        /// </summary>
        EnsureFullEmptyLineAfterMultilineItems,


        /// <summary>
        /// Writes a custom separator after the item
        /// </summary>
        WriteCustomSeparator,

        /// <summary>
        /// Don't write any separator after the item.
        /// </summary>
        None,

    }
    #endregion

}



#region "Internal/Hidden" Interfaces (those that should NOT be implemented by user code and shouldn't even be noticed) - most are just used for generics/extensions magic or for avoiding reflection
namespace CodegenCS.___InternalInterfaces___
{
    public interface IInlineIEnumerable
    {
        object Items { get; }
        RenderEnumerableOptions RenderOptions { get; }
    }
}
#endregion