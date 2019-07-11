Unisave Framework
=================

Unisave Framework = All the classes, that user-defined server-side code can talk to (server API).

However this does not mean it's not present on the client. It is, because it needs to simulated during development.

And since it's present on the client side, it's used for more than just server simulation.

But the primary purpouse is still - the stuff server-side code talks to.

The framework does not talk to the database directly, it has at it's disposal a set of interfaces to do that.
    (otherwise it couldn't be simulated on the client)
