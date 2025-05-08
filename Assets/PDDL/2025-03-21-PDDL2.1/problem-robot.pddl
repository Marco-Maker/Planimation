(define (problem pb1)
  (:domain robot)
	(:objects
      roomA - room roomB - room roomC - room
      ball1 - obj ball2 - obj eve - robot
      wally - robot)
	(:init
		(at-robot wally roomA)
		(at-robot eve roomB)
		(free wally)
		(free eve)
		(at-obj ball1 roomA)
		(at-obj ball2 roomA)
		(allowed wally roomA)
		(allowed wally roomB)
		(allowed eve roomB)
		(allowed eve roomC)
		)
	(:goal (and (at-obj ball1 roomC)
		(at-obj ball2 roomC)))
)
