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

**NATS CLI Context** - local CLI context for bob
```
{
  "description": "NATS Local Bob",
  "url": "nats://localhost:4222",
  "socks_proxy": "",
  "token": "",
  "user": "bob",
  "password": "bob",
  "creds": "",
  "nkey": "",
  "cert": "",
  "key": "",
  "ca": "",
  "nsc": "",
  "jetstream_domain": "",
  "jetstream_api_prefix": "",
  "jetstream_event_prefix": "",
  "inbox_prefix": "",
  "user_jwt": "",
  "color_scheme": "",
  "tls_first": false
}
```

Send a message, as Bob
```
nats pub bob.test "test"
```
