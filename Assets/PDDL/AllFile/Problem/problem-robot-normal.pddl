(define (problem pb1)
	(:domain robot)
	(:objects
		roomA - room
		roomB - room
		roomC - room
		roomD - room
		roomE - room
		roomF - room
		roomG - room
		roomH - room
		ball1 - obj
		ball2 - obj
		eve - robot
		wally - robot
	)
	(:init
		(at-robot wally roomA)
		(at-robot eve roomH)

		(free wally)
		(free eve)

		(at-obj ball1 roomA)
		(at-obj ball2 roomA)

		(allowed wally roomA)
		(allowed wally roomB)
		(allowed wally roomC)
		(allowed wally roomD)
		(allowed wally roomG)
		(allowed eve roomB)
		(allowed eve roomE)
		(allowed eve roomF)
		(allowed eve roomG)
		(allowed eve roomH)
		(connected roomA roomC)
		(connected roomC roomA)
		(connected roomC roomB)
		(connected roomB roomC)
		(connected roomC roomD)
		(connected roomD roomC)
		(connected roomG roomD)
		(connected roomD roomG)
		(connected roomB roomE)
		(connected roomE roomB)
		(connected roomF roomE)
		(connected roomE roomF)
		(connected roomF roomG)
		(connected roomG roomF)
		(connected roomF roomH)
		(connected roomH roomF)
	)
	(:goal
		(and (at-obj ball1 roomH)
			(at-obj ball2 roomH))
	)
)