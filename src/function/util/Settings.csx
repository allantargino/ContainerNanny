public static class Settings
{
    public static class ServiceBusQueue
        {
            public static string ConnectionString = "Endpoint=sb://visouza.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=T4oNGgxUt5ixT2BwDHg7qxBPjSi6hfRv3yEJqcScseE=";

            public static string QueueName = "Main";

            public static int MessageDelay = 2500;
        }

        public static class Kubernetes
        {
            public static string Server = "https://visouzak8smgmt.westus.cloudapp.azure.com";

            public static string AccessToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJkZWZhdWx0Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZWNyZXQubmFtZSI6ImRlZmF1bHQtdG9rZW4tcjQxemoiLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC5uYW1lIjoiZGVmYXVsdCIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VydmljZS1hY2NvdW50LnVpZCI6IjNiMTIwNjU1LWQ1ZmYtMTFlNy1hMDY0LTAwMGQzYTM2ZjNiNyIsInN1YiI6InN5c3RlbTpzZXJ2aWNlYWNjb3VudDpkZWZhdWx0OmRlZmF1bHQifQ.shSAGg_6OH9sM57GKgIu0eMSVQKsiRLvpOCwGm_vdEj5nNkaCwh1cd6I6_4K0GnWNScEXM9rwttAY18rHYq9mtgHCWGpO953RXqkjY69NnJpBI1ZGkitoHRSo8_nxsWbJbAaGtDtjqf5OJ8KI9OW_CQrx79toV9Kx23SkBjCh3NGTFc3q5pUs_fflYRgsCMDKSs08UhKLNMoFXrgS-NOUQXKJs74I3Hk4e9gVOfOBFdvrs6Lhdtfy2ZWlSmcrb9O2BLlX4T_P5TrsHwLwowtBO3CfaW8YRdDLR6ZLHpMQn2cvti73h0uJSxH2iubdE8NTyzzSvBNfefqAk85HPNshQ";
        }
}