(define (problem problem)
<<<<<<< HEAD
	(:domain domain-elevator-normal-capacity-infinity)
	(:objects
		floor1 floor2 - floor
		person1 person2 - person
=======
	(:domain domain-elevator-numeric)
	(:objects
>>>>>>> origin/main
		elevator1 - elevator
		person1 - person
	)
	(:init
<<<<<<< HEAD
		(at-person person1 floor1)
		(at-person person2 floor1)
		(at-elevator elevator1 floor1)
		(target person1 floor2)
		(above floor2 floor1)
=======
		(= (at-elevator elevator1) 1)
		(= (at-person person1) 1)
		(= (floors ) 2)
		(= (max-load elevator1) 3)
		(= (capacity elevator1) 3)
		(= (weight person1) 1)
		(= (target person1) 2)
>>>>>>> origin/main
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
