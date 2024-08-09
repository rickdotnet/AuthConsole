# AuthConsole
A console application demoing a NATS auth-callout endpoint.

## Demo

Use the following corresponding config values.

**authConfig.json** - place in bin folder
```
{
  "NatsUrl": "nats://localhost:4222",
  "Username": "authuser",
  "Password": "changeme",
  "SigningKey": "SAAG7CVIJWAU2TNNZJL72JYZ5K6DEQRBZBYDOA3BKU2PPHVRNPIBX2JDOE"
}
```

**nats.conf** - example nats config
```
http_port: 8222
jetstream: enabled
accounts {
  AUTH: {
    users: [
      { user: authuser, pass: changeme }
    ]
  }
  APP: {
    jetstream: enabled
  }

  SYS: {
  }
}
system_account: SYS

authorization {
  auth_callout {
    issuer: AACILD5SO3CCBAT5JJVOTWLVBLABXLPP2Q76SI5KSUYFV7STPTALPAG2
    auth_users: [ authuser ]
    account: AUTH
  }
}
```
