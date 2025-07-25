# Deployment Guide

## Quick Deploy to Render (Free)

1. **Fork this repository** on GitHub
2. **Sign up for Render** at [render.com](https://render.com) (free account)
3. **Connect GitHub** to your Render account
4. **Create New Web Service** and select your forked repository
5. **Render auto-detects** the `render.yaml` configuration
6. **Deploy!** Your API will be live in minutes

## Environment Variables (Optional)

For production with real credentials, set these in Render dashboard:

```
AzureAd__TenantId=your-actual-tenant-id
AzureAd__ClientId=your-actual-client-id
AzureAd__ClientSecret=your-actual-client-secret
Slack__BotToken=xoxb-your-actual-bot-token
Slack__SigningSecret=your-actual-signing-secret
```

## Demo Mode

The application automatically runs in demo mode when:
- No real credentials are provided
- Demo credentials are detected
- Any credential is missing

Demo mode provides:
- 5 sample Azure AD users
- 3 sample Azure AD groups
- 5 sample Slack users
- 4 sample Slack channels
- Full API functionality without external dependencies
