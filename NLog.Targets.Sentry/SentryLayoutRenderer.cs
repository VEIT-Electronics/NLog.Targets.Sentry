using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace NLog.Targets.Sentry
{
   [ThreadAgnostic]
   [AmbientProperty("SentryTag")]
   public class SentryLayoutRenderer : WrapperLayoutRendererBase
   {
      public const string TAG_PREFIX = "SENTRY-TAG|";
      public string SentryTag { get; set; }

      #region Overrides of LayoutRenderer

      /// <summary>
      /// Renders the specified environmental information and appends it to the specified <see cref="T:System.Text.StringBuilder"/>.
      /// </summary>
      /// <param name="builder">The <see cref="T:System.Text.StringBuilder"/> to append the rendered data to.</param><param name="logEvent">Logging event.</param>
      protected override void Append(StringBuilder builder, LogEventInfo logEvent)
      {
         var content = base.RenderInner(logEvent);
         if (string.IsNullOrEmpty(SentryTag) || string.IsNullOrEmpty(content)) return;
         var name = TAG_PREFIX + SentryTag;
         if (logEvent.Properties.ContainsKey(name))
         {
            logEvent.Properties[name] = content;
         }
         else
         {
            logEvent.Properties.Add(name,content);
         }
      }

      /// <summary>
      /// Transforms the output of another layout.
      /// </summary>
      /// <param name="text">Output to be transform.</param>
      /// <remarks>
      /// If the <see cref="T:NLog.LogEventInfo"/> is needed, overwrite <see cref="M:NLog.LayoutRenderers.Wrappers.WrapperLayoutRendererBase.Append(System.Text.StringBuilder,NLog.LogEventInfo)"/>.
      /// </remarks>
      /// <returns>
      /// Transformed text.
      /// </returns>
      protected override string Transform(string text)
      {
         return text;
      }

      #endregion
   }
}
