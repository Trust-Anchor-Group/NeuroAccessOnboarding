NeuroAccessOnboarding
=========================

This repository contains a service that facilitates onboarding of simple Neuro-Access digital identities. During the onboarding process, the user
validates its e-mail address and phone number with an onboarding Neuron, who directs the app to the most suitable host for the Neuro-Access account.
If the user chooses to create a simple Neuro-Access digital identity (i.e. only containing the phone number and e-mail address provided) the
digital identity can be automatically approved, if the host Neuron is able to validate the information with the onboarding Neuron. This repository
contains a serivce that performs this task: It registers an *Identity Authenticator*, which authenticates such simple Neuro-Access digital
identities with the onboarding Neuron, and approves the applications automatically, if the information matches the information validated during
the onboarding process.
