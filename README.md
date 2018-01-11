Sentry Target for NLog
======================

**NLog Sentry** is a custom target for [NLog](http://nlog-project.org/) enabling you to send logging messages to the [Sentry](http://getsentry.com) logging service.

## Configuration

To use the Sentry target, simply add it an extension in the NLog.config file and place the NLog.Targets.Sentry.dll in the same location as the NLog.dll & NLog.config files.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.Targets.Sentry" />
  </extensions>

  <targets>
    <target name="sentry" type="Sentry" dsn="[your sentry dsn]" environment="[develop|test|production]" timeout="[hh:MM:ss]"
     layout="${message}${processid:sentrytag=pid}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Error" writeTo="sentry" />
  </rules>
</nlog>
```

The DSN attribute is required for the target to log to Sentry. The `environment` and `timeout` properties are options and will default to "develop" 
and "00:00:10" if not explicitly set.

By using ambient property `sentrytag=pid` value of `${processid}` is sent to Sentry as tag value with name `pid`.
