Feature: HomeAppliancesCost
User Story
As a resident from England
I need to know estimate of how much electrical appliances cost to run
So that it can help me to reduce the energy cost and save money.

Background: Check if Calculator is accessible
	Given The Home appliance cost calculator is running

@Cost @Appliances @Story:1234 @TC:TC-1234
Scenario Outline: Get energy cost
	Given I am a resident from '<country>'
	When I add the '<NoOfAppliances>' appliances and its average usage and the national average rate '<avgRate>'
	Then I should get the results table with daily, weekly, monthly, and yearly cost

Examples:
	| country          | NoOfAppliances | avgRate |
	| England          | 8              | 34      |
	| Scotland         | 10             | 67      |
	| Wales            | 5              | 67      |

@Cost
Scenario Outline: Get energy cost - Northern Ireland 
	Given I am a resident from '<country>'
	Then I should get the results message as 'The advice on this website doesn’t cover Northern Ireland'


Examples:
	| country          | NoOfAppliances | avgRate |
	| Northern Ireland | 0              | 0       |