Title: Neuro-Access settings
Description: Configures integration with the onboarding neuron for Neuro-Access identities.
Date: 2023-12-11
Author: Peter Waher
Master: /Master.md
Cache-Control: max-age=0, no-cache, no-store
JavaScript: /Sniffers/Sniffer.js
JavaScript: Settings.js
UserVariable: User
Privilege: Admin.Identity.NeuroAccess
Login: /Login.md

========================================================================

<form action="Settings.md" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Neuro-Access settings</legend>

For the automatic approval of *Neuro-Access* identities, the Neuron(R) needs to know what the domain name of the *onboarding* Neuron(R)
is. During the onboarding process, the app validates its e-mail address and phone number with the onboarding Neuron(R), and is then
redirected to the most appropriate Neuron(R) for hosting the account. To be able to validate a Neuro-Access identity application
automatically, the Neuron(R) needs to authenticate the corresponding information with that onboarding Neuron(R).

{{
if exists(Posted) then
(
	SetSetting("TAG.Identity.NeuroAccess.OnboardingNeuron",Str(Posted.OnboardingNeuron));

	TAG.Identity.NeuroAccess.NeuroAccessAuthenticator.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<p>
<label for="OnboardingNeuron">Onboarding Neuron(R): (domain name)</label>  
<input type="text" id="OnboardingNeuron" name="OnboardingNeuron" value='{{GetSetting("TAG.Identity.NeuroAccess.OnboardingNeuron","")}}' required title="Domain name of onboarding Neuron."/>
</p>

<button type="submit" class="posButton">Apply</button>
</fieldset>
</form>