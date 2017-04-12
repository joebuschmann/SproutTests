@chromedriver
Feature: SproutApp
	In order to validate the Sprout web app
	As a user of Sprout
	I want to manage my Twitter inbox

Background:
	Given I have logged into my Sprout account

Scenario: Compose a Twitter message
	Given I compose a Twitter message
	When I enter the following text
	"""
	Hello Sprout!
	"""
	And I send it
	Then the message will be sent successfully

Scenario: View incoming Tweets
	When I navigate to the Messages tab
	Then I should have messages in my smart inbox
	When I reply to the first tweet with the following text
	"""
	This is a test reply.
	"""
	Then the message will be sent successfully

Scenario: Schedule a Tweet
	Given I navigate to the Publishing tab
	And choose the Calendar option
	When I compose the message
	"""
	Future dated tweet
	"""
	And schedule the post for one hour from now
	And send it
	Then the following message should appear on the calendar
	"""
	Future dated tweet
	"""